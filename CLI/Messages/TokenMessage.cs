namespace CLI.Messages;

public class TokenMessage : UserMessage
{
    public TokenMessage(BotUser sender, string text) : base(sender, text)
    {
    }
}