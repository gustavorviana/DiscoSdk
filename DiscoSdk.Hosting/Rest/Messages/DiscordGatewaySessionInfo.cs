using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Messages
{
    /// <summary>
    /// Represents session start limit information from the Discord Gateway.
    /// </summary>
    public class DiscordGatewaySessionInfo
    {
        /// <summary>
        /// Gets or sets the maximum number of concurrent session starts allowed.
        /// </summary>
        [JsonPropertyName("max_concurrency")]
        public int MaxConcurrency { get; set; }

        /// <summary>
        /// Gets or sets the number of remaining session starts in the current window.
        /// </summary>
        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }

        /// <summary>
        /// Gets or sets the time (in milliseconds) after which the rate limit resets.
        /// </summary>
        [JsonPropertyName("reset_after")]
        public int ResetAfter { get; set; }

        /// <summary>
        /// Gets or sets the total number of session starts allowed in the current window.
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
