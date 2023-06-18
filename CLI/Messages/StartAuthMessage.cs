using Serilog;

namespace CLI.Messages;

public class StartAuthMessage : UserMessage
{
    private readonly long _appId;
    private readonly string _authUrl;

    public StartAuthMessage(BotUser sender, string text, long appId, string authUrl) : base(sender, text)
    {
        _appId = appId;
        _authUrl = authUrl;
    }

    protected override bool CanRespond() => Sender.State == State.WaitingStartAuth;

    protected override async Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        var getVkTokenLink =
            "https://oauth.vk.com/oauth/authorize" +
            $"?client_id={_appId}" +
            "&scope=65540" +
            $"&redirect_uri={_authUrl}" +
            $"?sender_id={Sender.SenderId}" +
            "&display=page&response_type=token&revoke=1";

        var linkMessage = "Чтобы войти ВКонтакте, нажмите ".EscapeMarkdown() +
                          $"[сюда]({getVkTokenLink.EscapeMarkdown()})";
                          
        await telegramController.SendTextAsync(Sender.SenderId, linkMessage);

        using var cts = new CancellationTokenSource();
        var botUserName = await telegramController.GetBotUserNameAsync();

        var authEndpoint = new AuthEndpoint(Sender.SenderId, _authUrl, botUserName);
        cts.CancelAfter(TimeSpan.FromMinutes(5));

        try
        {
            await authEndpoint.WaitForAuth(cts.Token);
        }
        catch (TaskCanceledException)
        {
            var timeout = "Истекло время ожидания авторизации. Отправьте /start для перезапуска бота".EscapeMarkdown();
            await telegramController.SendTextAsync(Sender.SenderId, timeout);
            Sender.State = State.WaitingStart;
            return;
        }

        Sender.VkToken = authEndpoint.Token;
        var successfulAuth = "Успешная авторизация! Нажмите кнопку для перехода в главное меню".EscapeMarkdown();
        // await telegramController.SendTextAsync(Sender.SenderId, successfulAuth, keyboardBuilder.Build(MainMenu));
        await telegramController.SendTextAsync(Sender.SenderId, successfulAuth);
    }
}