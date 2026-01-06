using DiscoSdk.Hosting.Gateway;

namespace DiscoSdk.Hosting
{
    public class DiscordClientConfig
    {
        public int? TotalShards { get; set; }
        public required string Token { get; set; }
        public required GatewayIntent Intents { get; set; }
    }
}
