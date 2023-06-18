using Serilog;

namespace CLI.Messages;

public class CreateAlbumMessage : UserMessage
{
    public CreateAlbumMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    protected override Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        throw new NotImplementedException();
    }

    protected override bool CanRespond() => Sender.State == State.WaitingAlbumName;
}