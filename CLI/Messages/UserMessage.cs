using Serilog;

namespace CLI.Messages;

public abstract class UserMessage
{
    protected string UnexpectedResponse =
        "Неожиданная команда. Попробуйте еще раз или отправьте /start для перезапуска бота".EscapeMarkdown();

    public BotUser Sender { get; }
    public string Text { get; }

    protected UserMessage(BotUser sender, string text)
    {
        Sender = sender;
        Text = text;
    }

    public async Task Respond(ILogger logger, VkManager vkManager, TelegramController telegramController)
    {
        if (CanRespond())
        {
            await RespondInternal(logger, vkManager, telegramController);
            logger.Information("{senderId}: Respond sent", Sender.SenderId);
            return;
        }

        await telegramController.SendTextAsync(Sender.SenderId, UnexpectedResponse);
        logger.Warning("{senderId}: Unexpected messagee! Respond sent", Sender.SenderId);
    }

    protected abstract Task RespondInternal(ILogger logger, VkManager vkManager, TelegramController telegramController);

    protected virtual bool CanRespond() => true;
}