using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Represents the response from the Discord Gateway endpoint.
/// </summary>
public class DiscordGatewayResponse
{
    /// <summary>
    /// Gets or sets the WebSocket gateway URL.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the session start limit information.
    /// </summary>
    [JsonPropertyName("session_start_limit")]
    public required SessionStartLimit SessionStartLimit { get; set; }

    /// <summary>
    /// Gets or sets the recommended number of shards.
    /// </summary>
    [JsonPropertyName("shards")]
    public int Shards { get; set; }
}
