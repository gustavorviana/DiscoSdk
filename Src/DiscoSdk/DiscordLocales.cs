namespace DiscoSdk;

public class DiscordLocales
{
    private static readonly HashSet<string> _validLocales = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "da", "de", "en-GB", "en-US", "es-ES", "fr", "hr", "it", "lt", "hu", "nl", "no", "pl", "pt-BR", "ro", "fi", "sv-SE", "vi", "tr", "cs", "el", "bg", "ru", "uk", "hi", "th", "zh-CN", "ja", "zh-TW", "ko"
    };

    public static string[] GetAll() => [.. _validLocales];

    public static bool Has(string key)
    {
        return _validLocales.Contains(key); 
    }
}