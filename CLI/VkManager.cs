using Serilog;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace CLI;

public class AlbumWrapper
{
    public AlbumWrapper(long id, string title)
    {
        Id = id;
        Title = title;
    }

    public long Id { get; }
    public string Title { get; }
}

public class PhotoWrapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath">Path to photo on disk. Can be <see langword="null"/> if <see cref="fileId"/> is set.</param>
    /// <param name="fileId">Telegram file_id. Can be <see langword="null"/> if <see cref="filePath"/> is set.</param>
    /// <param name="savedDateTime"></param>
    /// <param name="index"></param>
    /// <param name="totalCount"></param>
    public PhotoWrapper(string filePath, string fileId, DateTime savedDateTime, int index, int totalCount)
    {
        TelegramFileId = fileId;

        FilePath = filePath;
        SavedDateTime = savedDateTime;
        Index = index;
        TotalCount = totalCount;
    }

    public string TelegramFileId { get; }
    public string FilePath { get; }

    public int Index { get; }
    public int TotalCount { get; }
    public DateTime SavedDateTime { get; }
}

public class VkManager
{
    private readonly ILogger _logger;
    private const int SavedAlbumId = -15;

    private readonly VkApi _vkApi;
    private readonly HttpClient _httpClient;
    private User _authorizedUser;

    public VkManager(ILogger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _vkApi = new VkApi();
    }

    public string GetName(string vkToken)
    {
        if (!Auth(vkToken))
            throw new VkAuthorizationException();

        return _authorizedUser.FirstName + ' ' + _authorizedUser.LastName;
    }

    public IReadOnlyCollection<AlbumWrapper> GetAlbums(string vkToken)
    {
        if (!Auth(vkToken))
            throw new VkAuthorizationException();

        var customAlbums = GetCustomAlbums();
        return customAlbums.Select(album => new AlbumWrapper(album.Id, album.Title)).ToList();
    }

    public PhotoWrapper GetSavedPhoto(string vkToken, int photoIndex)
    {
        if (!Auth(vkToken))
            throw new VkAuthorizationException();

        var savedAlbum = GetSavedAlbum();
        var count = GetPicsCount(savedAlbum);

        // index = user.SortingMode switch
        // {
        //     SortingMode.Random => Random.Shared.Next(count),
        //     SortingMode.ToOlder => user.PhotoIndex - 1,
        //     SortingMode.ToNewer => user.IsLastPhotoSkipped ? user.PhotoIndex + 1 : user.PhotoIndex,
        //     /*     0 1 2 3 4
        //      * old     ^     new
        //      * if we move image, index does not change
        //      *
        //      *     0 1 (2) 2 3
        //      * old         ^   new
        //      * but if we skip image, index changes
        //      *     0 1 2 3 4
        //      * old       ^   new
        //      */
        // };
        //
        // if (index < 0)
        //     index = 0;
        //
        // if (index >= count)
        //     index = count - 1;

        var photo = GetPhotoAt(savedAlbum, photoIndex);
        ArgumentNullException.ThrowIfNull(photo);

        var filename = DownloadPic(photo);
        return new PhotoWrapper(filename, null, photo.CreateTime.GetValueOrDefault(), count - photoIndex, count);
    }

    public void MovePhotoToAlbum(string vkToken, long albumId, ulong photoId)
    {
        if (!Auth(vkToken))
            throw new VkAuthorizationException();

        _vkApi.Photo.Move(albumId, photoId);
    }

    private bool Auth(string vkToken)
    {
        var authParams = new ApiAuthParams()
        {
            AccessToken = vkToken
        };

        try
        {
            _vkApi.Authorize(authParams);
        }
        catch
        {
            return false;
        }

        _authorizedUser = _vkApi.Users.Get(Array.Empty<long>()).First();
        return true;
    }

    private Photo GetPhotoAt(PhotoAlbum album, int index)
    {
        var photoGetParams = new PhotoGetParams()
        {
            AlbumId = PhotoAlbumType.Id(album.Id),
            OwnerId = _authorizedUser.Id,
            Offset = (ulong) index,
            Count = 1
        };

        var pics = SafeVkRequest(_vkApi.Photo.Get, photoGetParams);

        return pics.FirstOrDefault();
    }

    private int GetPicsCount(PhotoAlbum album)
    {
        var photoGetParams = new PhotoGetParams()
        {
            AlbumId = PhotoAlbumType.Id(album.Id),
            OwnerId = _authorizedUser.Id,
            Offset = 0,
            Count = 1
        };

        var pics = SafeVkRequest(_vkApi.Photo.Get, photoGetParams);

        return (int) pics.TotalCount;
    }

    private IEnumerable<PhotoAlbum> GetCustomAlbums()
    {
        const int count = 1000;
        List<PhotoAlbum> allAlbums = new();
        for (uint? i = 0;; i += count)
        {
            var photoGetAlbumParams = new PhotoGetAlbumsParams()
            {
                OwnerId = _authorizedUser.Id,
                Offset = i,
                Count = count,
            };

            var albums = SafeVkRequest(_vkApi.Photo.GetAlbums, photoGetAlbumParams)
                .Where(a => a.Id > 0)
                .ToList();

            if (!albums.Any())
            {
                return allAlbums;
            }

            allAlbums.AddRange(albums);
        }
    }

    private PhotoAlbum GetSavedAlbum()
    {
        const int count = 1000;
        var photoGetAlbumsParams = new PhotoGetAlbumsParams()
        {
            OwnerId = _authorizedUser.Id,
            NeedSystem = true,
            Offset = 0,
            Count = count
        };

        var savedAlbum =
            SafeVkRequest(_vkApi.Photo.GetAlbums, photoGetAlbumsParams)
                .FirstOrDefault(a => a.Id == SavedAlbumId);

        return savedAlbum;
    }

    private string DownloadPic(Photo pic)
    {
        const string filename = "image.jpg";

        PhotoSize size;
        if (pic.Width <= pic.Height)
        {
            var sizes = pic.Sizes.OrderBy(x => x.Width).ToList();
            size = sizes.FirstOrDefault(x => x.Width > 500) ?? sizes.Last();
        }
        else
        {
            var sizes = pic.Sizes.OrderBy(x => x.Height).ToList();
            size = sizes.FirstOrDefault(x => x.Height > 500) ?? sizes.Last();
        }

        using var response = _httpClient.Send(new HttpRequestMessage(HttpMethod.Get, size.Url));
        using var stream = response.Content.ReadAsStream();
        using var fs = File.Create(filename);
        stream.CopyTo(fs);

        return filename;
    }

    private static VkCollection<T> SafeVkRequest<T, P>(Func<P, bool, VkCollection<T>> request, P requestParams)
    {
        while (true)
        {
            try
            {
                var entities = request(requestParams, false);
                return entities;
            }
            catch (TooManyRequestsException)
            {
                Thread.Sleep(200);
            }
        }
    }
}