namespace CLI.Messages;

public class NewSortingModeMessage : UserMessage
{
    public NewSortingModeMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}