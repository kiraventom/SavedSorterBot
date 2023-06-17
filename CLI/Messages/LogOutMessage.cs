namespace CLI.Messages;

public class LogOutMessage : UserMessage
{
    public LogOutMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}