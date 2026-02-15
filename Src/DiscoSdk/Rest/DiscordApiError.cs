namespace DiscoSdk.Rest;

public sealed class DiscordApiError
{
    public string? Message { get; init; }

    public int? Code { get; init; }

    /// <summary>
    /// Normalized list of validation errors (flattened from Discord's error payload).
    /// </summary>
    public IReadOnlyList<DiscordValidationError> ValidationErrors { get; init; }
        = Array.Empty<DiscordValidationError>();
}