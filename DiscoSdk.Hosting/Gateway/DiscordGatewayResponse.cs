using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway;

public class DiscordGatewayResponse
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("session_start_limit")]
    public required SessionStartLimit SessionStartLimit { get; set; }

    [JsonPropertyName("shards")]
    public int Shards { get; set; }
}
