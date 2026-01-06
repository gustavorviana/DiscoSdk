namespace DiscoSdk.Hosting.Rest;

public sealed class DiscordErrorResponse
{
    public int? Code { get; init; }
    public string? Message { get; init; }
}