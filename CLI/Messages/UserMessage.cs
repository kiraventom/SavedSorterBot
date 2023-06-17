using Serilog;

namespace CLI.Messages;

public abstract class UserMessage
{
    public BotUser Sender { get; }
    public string Text { get; }

    protected UserMessage(BotUser sender, string text)
    {
        Text = text;
    }
    
    public async virtual Task Respond(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        
    }
}
