using CLI.Messages;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CLI;


public class TelegramController
{
    private readonly ILogger _logger;
    private readonly KeyboardBuilder _keyboardBuilder;
    private readonly UserDatabase _userDatabase;

    public event Action<UserMessage> UserMessageReceived;

    public TelegramController(string botToken, ILogger logger, UserDatabase userDatabase)
    {
        _logger = logger;
        _userDatabase = userDatabase;
        _keyboardBuilder = new KeyboardBuilder();

        var botClient = new TelegramBotClient(botToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] {UpdateType.Message}
        };

        botClient.StartReceiving(OnUpdateReceived, OnErrorReceived, receiverOptions);
        _logger.Information("Started listening");
    }
    
    public async Task SendText(long senderId, string text)
    {
        var escapedText = Utils.Escape(text);
    }
    
    public async Task SendPhoto(long senderId, string filepath, string text)
    {
        var escapedText = Utils.Escape(text);
    }

    private async Task OnUpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(update);
        _logger.Information("Received update: {%update}", update);

        var message = update.Message;
        if (message is null)
            return;

        var chat = message.Chat;
        var senderId = chat.Id;
        if (senderId < 0) // is group or channel
            return;

        var text = message.Text;
        if (text is null)
            return;
            
        var name = GetNameFromSender(message.From);
        _logger.Information("{senderId} {name}: Received text message \"{text}\"", senderId, name, text);

        var sender = _userDatabase.Get(senderId);
        UserMessage userMessage = text switch
        {
            "/start" => new StartMessage(sender, text),
            KeyboardBuilder.StartSorting => new StartSortingMessage(sender, text),
            KeyboardBuilder.SkipImage => new SkipImageMessage(sender, text),
            KeyboardBuilder.DeleteImage => new DeleteImageMessage(sender, text),
            KeyboardBuilder.CreateAlbum => new CreateAlbumMessage(sender, text),
            KeyboardBuilder.MainMenu => new MainMenuMessage(sender, text),
            KeyboardBuilder.Settings => new SettingsMessage(sender, text),
            KeyboardBuilder.LogOut => new LogOutMessage(sender, text),
            { } s when s.Contains("oauth.vk.com") => new TokenMessage(sender, text),
            { } when sender.State is State.WaitingAlbumName => new MoveImageToAlbumMessage(sender, text),
            { } when sender.State is State.WaitingSortingMode => new NewSortingModeMessage(sender, text),
            { } when sender.State is State.WaitingNewAlbumName => new NewAlbumNameMessage(sender, text),
            _ => new UnexpectedMessage(sender, text),
        };
        
        _logger.Information("User message detected as {type}", userMessage.GetType().Name);
        
        UserMessageReceived?.Invoke(userMessage);
    }

    private Task OnErrorReceived(ITelegramBotClient botClient, Exception exception, CancellationToken token)
    {
        throw new NotImplementedException();
    }
    
    private static string GetNameFromSender(User user)
    {
        return user.Username is not null
            ? '@' + user.Username
            : user.FirstName + ' ' + user.LastName;
    }
}

public class KeyboardBuilder
{
    public const string StartSorting = "Начать сортировку";
    public const string SkipImage = "Пропустить";
    public const string DeleteImage = "Удалить";
    public const string CreateAlbum = "В новый альбом";
    public const string MainMenu = "В главное меню";
    public const string Settings = "Настройки";
    public const string LogOut = "Выйти из аккаунта";
}