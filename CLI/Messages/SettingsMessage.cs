namespace CLI.Messages;

public class SettingsMessage : UserMessage
{
    public SettingsMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}