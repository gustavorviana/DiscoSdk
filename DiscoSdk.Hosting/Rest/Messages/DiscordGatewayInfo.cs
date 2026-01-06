using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Messages
{
    /// <summary>
    /// Represents gateway information retrieved from the Discord API.
    /// </summary>
    internal class DiscordGatewayInfo
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
        public required DiscordGatewaySessionInfo SessionInfo { get; set; }

        /// <summary>
        /// Gets or sets the recommended number of shards.
        /// </summary>
        [JsonPropertyName("shards")]
        public int Shards { get; set; }
    }
}
