namespace CLI.Messages;

public class MoveImageToAlbumMessage : UserMessage
{
    public MoveImageToAlbumMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}