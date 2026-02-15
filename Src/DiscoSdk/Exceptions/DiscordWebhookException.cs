namespace DiscoSdk.Exceptions;

public sealed class DiscordWebhookException : DiscoException
{
    public int StatusCode { get; }
    public string? Reason { get; }
    public string? ResponseBody { get; }

    public DiscordWebhookException(int statusCode, string? reason, string? responseBody)
        : base(BuildMessage(statusCode, reason, responseBody))
    {
        StatusCode = statusCode;
        Reason = reason;
        ResponseBody = responseBody;
    }

    private static string BuildMessage(int statusCode, string? reason, string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return $"Discord webhook request failed: {statusCode} {reason}";

        return $"Discord webhook request failed: {statusCode} {reason}\n{body}";
    }
}
