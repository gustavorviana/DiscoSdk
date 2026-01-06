using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Messages
{
    public class DiscordGatewaySessionInfo
    {
        [JsonPropertyName("max_concurrency")]
        public int MaxConcurrency { get; set; }

        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }

        [JsonPropertyName("reset_after")]
        public int ResetAfter { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
