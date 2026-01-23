namespace DiscoSdk.Hosting.Rest;

/// <summary>
/// Represents an error response from the Discord API.
/// </summary>
public sealed class DiscordErrorResponse
{
    /// <summary>
    /// Gets the Discord error code, if available.
    /// </summary>
    public int? Code { get; init; }

    /// <summary>
    /// Gets the error message from Discord.
    /// </summary>
    public string? Message { get; init; }
}