using Serilog;

namespace CLI.Messages;

public class SkipImageMessage : UserMessage
{
    public SkipImageMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    protected override Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        throw new NotImplementedException();
    }

    protected override bool CanRespond() => Sender.State == State.WaitingAlbumName;
}