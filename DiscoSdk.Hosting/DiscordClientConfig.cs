using DiscoSdk.Hosting.Gateway;

namespace DiscoSdk.Hosting
{
    /// <summary>
    /// Configuration settings for the Discord client.
    /// </summary>
    public class DiscordClientConfig
    {
        /// <summary>
        /// Gets or sets the total number of shards. If null, the value will be determined from the gateway.
        /// </summary>
        public int? TotalShards { get; set; }

        /// <summary>
        /// Gets or sets the bot token for authentication.
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// Gets or sets the gateway intents to subscribe to.
        /// </summary>
        public required GatewayIntent Intents { get; set; }
    }
}
