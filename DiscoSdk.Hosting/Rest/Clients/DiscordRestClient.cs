using DiscoSdk.Hosting.Rest.Messages;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for making REST API requests to Discord.
/// </summary>
internal class DiscordRestClient(IDiscordRestClientBase client)
{
    /// <summary>
    /// Retrieves gateway information for the bot, including the WebSocket URL and session limits.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains gateway information.</returns>
    public Task<DiscordGatewayInfo> GetGatewayBotInfoAsync(CancellationToken ct = default)
    {
        return client.SendAsync<DiscordGatewayInfo>("gateway/bot", HttpMethod.Get, body: null, ct);
    }
}
