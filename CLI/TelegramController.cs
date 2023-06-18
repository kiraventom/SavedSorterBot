using CLI.Messages;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CLI;

public class TelegramController
{
    private readonly BotConfig _botConfig;
    private readonly ILogger _logger;
    private readonly KeyboardBuilder _keyboardBuilder;
    private readonly UserDatabase _userDatabase;
    private readonly TelegramBotClient _botClient;

    public event Action<UserMessage> UserMessageReceived;

    public TelegramController(BotConfig botConfig, ILogger logger, UserDatabase userDatabase)
    {
        _botConfig = botConfig;
        _logger = logger;
        _userDatabase = userDatabase;
        _keyboardBuilder = new KeyboardBuilder();
        _botClient = new TelegramBotClient(botConfig.BotToken);
    }

    public void StartListening()
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] {UpdateType.Message}
        };

        _botClient.StartReceiving(OnUpdateReceived, OnErrorReceived, receiverOptions);
        _logger.Information("Started listening");
    }

    public async Task<string> GetBotUserNameAsync()
    {
        var bot = await _botClient.GetMeAsync();
        return bot.Username;
    }

    public async Task SendTextAsync(long senderId, string text)
    {
        await _botClient.SendTextMessageAsync(senderId, text, null, ParseMode.MarkdownV2);
    }

    public async Task SendPhotoAsync(long senderId, string filepath, string text)
    {
        var escapedText = BotUtils.EscapeMarkdown(text);
    }

    private async Task OnUpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(update);
        _logger.Information("Received update: {%update}", update);

        if (!TryGetSenderId(update, out var senderId))
            return;

        if (!TryGetText(update, out var text))
            return;

        var name = GetNameFromSender(update!.Message!.From);
        _logger.Information("{senderId} {name}: Received text message \"{text}\"", senderId, name, text);

        var sender = _userDatabase.Get(senderId);
        HandleTextMessage(sender, text);
    }

    private static bool TryGetSenderId(Update update, out long senderId)
    {
        senderId = long.MaxValue;

        var message = update.Message;
        if (message is null)
            return false;

        var chat = message.Chat;
        senderId = chat.Id;
        return senderId >= 0;
    }

    private static bool TryGetText(Update update, out string text)
    {
        text = update!.Message!.Text;
        return text is not null;
    }

    private void HandleTextMessage(BotUser sender, string text)
    {
        UserMessage userMessage = text switch
        {
            null => new UnexpectedMessage(sender, text),
            "/start" => new StartMessage(sender, text),

            KeyboardBuilder.StartAuth => new StartAuthMessage(sender, text, _botConfig.AppId, _botConfig.RedirectUrl),
            KeyboardBuilder.StartSorting => new StartSortingMessage(sender, text),
            KeyboardBuilder.Settings => new SettingsMessage(sender, text),
            KeyboardBuilder.LogOut => new LogOutMessage(sender, text),
            KeyboardBuilder.SkipImage => new SkipImageMessage(sender, text),
            KeyboardBuilder.DeleteImage => new DeleteImageMessage(sender, text),
            KeyboardBuilder.CreateAlbum => new CreateAlbumMessage(sender, text),
            KeyboardBuilder.MainMenu => new MainMenuMessage(sender, text),
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
        _logger.Error(exception.Message);
        throw exception;
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
    public const string StartAuth = "Авторизоваться ВКонтакте";
    public const string StartSorting = "Начать сортировку";
    public const string SkipImage = "Пропустить";
    public const string DeleteImage = "Удалить";
    public const string CreateAlbum = "В новый альбом";
    public const string MainMenu = "В главное меню";
    public const string Settings = "Настройки";
    public const string LogOut = "Выйти из аккаунта";
}