using DiscoSdk.Hosting.Rest.Messages;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for making REST API requests to Discord.
/// </summary>
internal class DiscordGatewayClient(IDiscordRestClient client)
{
    /// <summary>
    /// Retrieves gateway information for the bot, including the WebSocket URL and session limits.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains gateway information.</returns>
    public Task<DiscordGatewayInfo> GetGatewayBotInfoAsync(CancellationToken ct = default)
    {
        var route = new DiscordRoute("gateway/bot");
        return client.SendAsync<DiscordGatewayInfo>(route, HttpMethod.Get, body: null, ct);
    }
}
