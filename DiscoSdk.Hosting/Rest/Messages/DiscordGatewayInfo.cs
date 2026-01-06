using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Messages
{
    internal class DiscordGatewayInfo
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("session_start_limit")]
        public required DiscordGatewaySessionInfo SessionInfo { get; set; }

        [JsonPropertyName("shards")]
        public int Shards { get; set; }
    }
}
