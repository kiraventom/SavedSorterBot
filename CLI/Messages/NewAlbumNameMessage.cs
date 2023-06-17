namespace CLI.Messages;

public class NewAlbumNameMessage : UserMessage
{
    public NewAlbumNameMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}