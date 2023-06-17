namespace CLI;

public static class Utils
{
    private static readonly string[] CharsToEscape = { "!", ".", "(", ")", "-"  };
    
    public static string Escape(string text)
    {
        foreach (var charToEscape in CharsToEscape)
        {
            text = text.Replace(charToEscape, $"\\{charToEscape}");
        }
        
        return text;
    }
}