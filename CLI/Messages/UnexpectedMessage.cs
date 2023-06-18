using Serilog;

namespace CLI.Messages;

public class UnexpectedMessage : UserMessage
{
    public UnexpectedMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    protected override async Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        await telegramController.SendTextAsync(Sender.SenderId, UnexpectedResponse);
    }
}