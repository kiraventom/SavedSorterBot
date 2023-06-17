using Serilog;

namespace CLI.Messages;

public class StartMessage : UserMessage
{
    public StartMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    public async override Task Respond(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        const string message = "При помощи этого бота можно отсортировать альбом \"Сохраненные фотографии\" по альбомам";
        await telegramController.SendText(Sender.SenderId, message);
    }
}