using System.Text.Json;
using System.Text.Json.Serialization;

namespace CLI;

public class BotUser
{
    private string _vkToken;
    private SortingMode _sortingMode;
    private int _photoIndex;
    private State _state;

    public long SenderId { get; }

    public string VkToken
    {
        get => _vkToken;
        set => SetAndRaise(ref _vkToken, value);
    }

    public SortingMode SortingMode
    {
        get => _sortingMode;
        set => SetAndRaise(ref _sortingMode, value);
    }

    public int PhotoIndex
    {
        get => _photoIndex;
        set => SetAndRaise(ref _photoIndex, value);
    }

    public State State
    {
        get => _state;
        set => SetAndRaise(ref _state, value);
    }

    public event Action PropertyChanged;

    [JsonConstructor]
    public BotUser(long senderId, string vkToken, SortingMode sortingMode, int photoIndex, State state)
    {
        SenderId = senderId;
        VkToken = vkToken;
        SortingMode = sortingMode;
        PhotoIndex = photoIndex;
        State = state;
    }

    private void SetAndRaise<T>(ref T field, T value)
    {
        if (Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke();
    }
}

public class UserDatabase
{
    private readonly string _filename;
    private readonly Dictionary<long, BotUser> _users;

    public UserDatabase(string filename)
    {
        _filename = filename;
        _users = ReadFromFile(filename);

        foreach (var user in _users.Values)
            user.PropertyChanged += OnUserPropertyChanged;
    }

    public BotUser Get(long senderId)
    {
        if (_users.TryGetValue(senderId, out var user))
            return user;
            
        user = new BotUser(senderId, string.Empty, SortingMode.Random, 0, State.WaitingStart);
        _users.Add(senderId, user);
        user.PropertyChanged += OnUserPropertyChanged;
        return user;
    }

    public void Remove(long senderId)
    {
        var oldUser = _users[senderId];
        oldUser.PropertyChanged -= OnUserPropertyChanged;
        _users.Remove(senderId);
    }

    private void OnUserPropertyChanged()
    {
        WriteToFile(_filename, _users);
    }

    private static Dictionary<long, BotUser> ReadFromFile(string filename)
    {
        using var file = File.OpenRead(filename);
        return JsonSerializer.Deserialize<Dictionary<long, BotUser>>(file, BotUtils.DefaultJsonOptions);
    }

    private static void WriteToFile(string filename, Dictionary<long, BotUser> users)
    {
        using var file = File.Create(filename);
        JsonSerializer.Serialize(file, users, BotUtils.DefaultJsonOptions);
    }
}