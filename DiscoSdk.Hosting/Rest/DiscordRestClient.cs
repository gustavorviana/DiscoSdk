using DiscoSdk.Hosting.Rest.Messages;

namespace DiscoSdk.Hosting.Rest
{
    internal class DiscordRestClient(IDiscordRestClientBase client)
    {
        public Task<DiscordGatewayInfo> GetGatewayBotInfoAsync(CancellationToken ct = default)
        {
            return client.SendJsonAsync<DiscordGatewayInfo>("gateway/bot", HttpMethod.Get, body: null, ct);
        }
    }
}
