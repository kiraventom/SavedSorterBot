namespace CLI.Messages;

public class DeleteImageMessage : UserMessage
{
    public DeleteImageMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}