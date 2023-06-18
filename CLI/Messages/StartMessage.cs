using Serilog;

namespace CLI.Messages;

public class StartMessage : UserMessage
{
    public StartMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    protected override async Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        var greeting = "При помощи этого бота можно отсортировать альбом \"Сохраненные фотографии\" по альбомам.\n".EscapeMarkdown() +
                                $"Для продолжения нажмите кнопку \"{KeyboardBuilder.StartAuth}\"".EscapeMarkdown();
                                
        // await telegramController.SendTextAsync(Sender.SenderId, greeting, keyboardBuilder.Build(KeyboardBuilder.StartAuth));
        await telegramController.SendTextAsync(Sender.SenderId, greeting);
        Sender.State = State.WaitingStartAuth;
    }
}