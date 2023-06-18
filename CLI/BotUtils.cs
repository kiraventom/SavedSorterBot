using System.Text.Json;

namespace CLI;

public static class BotUtils
{
    private static readonly string[] CharsToEscape = {"!", ".", "(", ")", "-", "=", "_", "*"};

    public static string EscapeMarkdown(this string text)
    {
        return CharsToEscape.Aggregate(text,
            (current, charToEscape) => current.Replace(charToEscape, $"\\{charToEscape}"));
    }

    public static JsonSerializerOptions DefaultJsonOptions { get; } = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true
    };
}