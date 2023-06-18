using Serilog;

namespace CLI.Messages;

public class MoveImageToAlbumMessage : UserMessage
{
    public MoveImageToAlbumMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    protected override Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        throw new NotImplementedException();
    }
}