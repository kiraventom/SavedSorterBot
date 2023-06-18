using Serilog;

namespace CLI.Messages;

public class MainMenuMessage : UserMessage
{
    public MainMenuMessage(BotUser sender, string text) : base(sender, text)
    {
    }

    protected override Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        throw new NotImplementedException();
    }

    protected override bool CanRespond() => Sender.State is State.WaitingAlbumName or State.WaitingSortingMode;
}