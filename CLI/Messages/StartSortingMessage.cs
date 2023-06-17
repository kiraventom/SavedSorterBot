namespace CLI.Messages;

public class StartSortingMessage : UserMessage
{
    public StartSortingMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}