namespace CLI.Messages;

public class SkipImageMessage : UserMessage
{
    public SkipImageMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}