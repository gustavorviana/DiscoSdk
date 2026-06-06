namespace DiscoSdk;

/// <summary>
/// Replaces sensitive credential material in strings before they reach a logger, an exception
/// message, or a serialisation surface. Bot tokens leaking into logs is one of the most common
/// abuse vectors in Discord-bot deploys — a single grep in a shared log aggregator (Datadog,
/// Sentry, ELK) is enough to compromise a bot for a hostile actor.
/// </summary>
public static class TokenSanitizer
{
    private const int VisibleSuffix = 4;
    private const char Placeholder = '*';

    /// <summary>
    /// Masks an opaque token string, keeping only the last <see cref="VisibleSuffix"/> characters
    /// so a human can still correlate a log entry with a specific bot without exposing the
    /// secret. Empty / whitespace inputs are returned as-is so the function is safe to wrap
    /// around any user-supplied configuration value.
    /// </summary>
    public static string Mask(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return token ?? string.Empty;

        if (token.Length <= VisibleSuffix)
            return new string(Placeholder, token.Length);

        var hiddenLength = token.Length - VisibleSuffix;
        return string.Concat(new string(Placeholder, Math.Min(hiddenLength, 12)), token[^VisibleSuffix..]);
    }

    /// <summary>
    /// Convenience: masks any embedded <c>Bot XXX</c>, <c>Bearer XXX</c>, or raw token-shaped
    /// substring in a free-form text payload (HTTP request dump, exception message). Falls back
    /// to identity when no known pattern is present so callers can pipe arbitrary strings
    /// through without conditional logic.
    /// </summary>
    public static string MaskAll(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        // Bot {token} and Bearer {token} are the two Authorization-header forms Discord uses.
        var masked = MaskAfterPrefix(text, "Bot ");
        masked = MaskAfterPrefix(masked, "Bearer ");
        return masked;
    }

    private static string MaskAfterPrefix(string text, string prefix)
    {
        var idx = text.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        while (idx >= 0)
        {
            var start = idx + prefix.Length;
            var end = start;
            while (end < text.Length && !char.IsWhiteSpace(text[end]) && text[end] != '"')
                end++;

            if (end == start)
                break;

            var token = text[start..end];
            text = string.Concat(text.AsSpan(0, start), Mask(token), text.AsSpan(end));
            idx = text.IndexOf(prefix, end, StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }
}
