using DiscoSdk.Hosting.Gateway.Payloads.Models;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway.Payloads;

internal sealed class ReadyPayload
{
    /// <summary>
    /// API version.
    /// </summary>
    [JsonPropertyName("v")]
    public int Version { get; set; }

    /// <summary>
    /// Information about the user including email.
    /// </summary>
    [JsonPropertyName("user")]
    public ReadyUser User { get; set; } = default!;

    /// <summary>
    /// Guilds the user is in (unavailable guild objects).
    /// </summary>
    [JsonPropertyName("guilds")]
    public List<UnavailableGuild> Guilds { get; set; } = new();

    /// <summary>
    /// Used for resuming connections.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = default!;

    /// <summary>
    /// Gateway URL for resuming connections.
    /// </summary>
    [JsonPropertyName("resume_gateway_url")]
    public string ResumeGatewayUrl { get; set; } = default!;

    /// <summary>
    /// Shard information associated with this session, if sent when identifying.
    /// [ shard_id, num_shards ]
    /// </summary>
    [JsonPropertyName("shard")]
    public int[]? Shard { get; set; }

    /// <summary>
    /// Partial application object (id and flags).
    /// </summary>
    [JsonPropertyName("application")]
    public ReadyApplication Application { get; set; } = default!;
}
