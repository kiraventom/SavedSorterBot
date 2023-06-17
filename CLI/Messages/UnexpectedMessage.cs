namespace CLI.Messages;

public class UnexpectedMessage : UserMessage
{
    public UnexpectedMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}