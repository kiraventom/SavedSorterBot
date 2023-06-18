using Serilog;

namespace CLI.Messages;

public class NewSortingModeMessage : UserMessage
{
    public NewSortingModeMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    protected override Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        throw new NotImplementedException();
    }
}