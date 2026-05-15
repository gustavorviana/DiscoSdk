using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IWebhooks"/>. Delegates to <see cref="WebhookClient"/>.
/// </summary>
internal sealed class WebhooksSurface(DiscordClient client) : IWebhooks
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public IRestAction<IWebhook?> Get(Snowflake webhookId)
        => RestAction<IWebhook?>.Create(async ct =>
        {
            var webhook = await _client.WebhookClient.GetAsync(webhookId, ct);
            return webhook is null ? null : new WebhookWrapper(_client, webhook);
        });

    /// <inheritdoc />
    public IRestAction<IWebhook?> Get(Snowflake webhookId, string token)
        => RestAction<IWebhook?>.Create(async ct =>
        {
            var webhook = await _client.WebhookClient.GetWithTokenAsync(webhookId, token, ct);
            return webhook is null ? null : new WebhookWrapper(_client, webhook);
        });

    /// <inheritdoc />
    public ICreateWebhookAction Create(Snowflake channelId) => new CreateWebhookAction(_client, channelId);

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<IWebhook>> ListForChannel(Snowflake channelId)
        => RestAction<IReadOnlyList<IWebhook>>.Create(async ct =>
        {
            var webhooks = await _client.WebhookClient.GetChannelWebhooksAsync(channelId, ct);
            return webhooks.Select(w => (IWebhook)new WebhookWrapper(_client, w)).ToList().AsReadOnly();
        });
}
