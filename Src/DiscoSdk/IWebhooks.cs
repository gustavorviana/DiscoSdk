using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Public surface for webhook operations not naturally owned by a channel/guild entity. Exposed
/// via <see cref="IDiscordClient.Webhooks"/>.
/// </summary>
public interface IWebhooks
{
    /// <summary>Resolves a webhook by ID. Returns <c>null</c> if the webhook does not exist.</summary>
    IRestAction<IWebhook?> Get(Snowflake webhookId);

    /// <summary>Resolves a webhook by ID and token (no permission check). Returns <c>null</c> if not found.</summary>
    IRestAction<IWebhook?> Get(Snowflake webhookId, string token);

    /// <summary>Creates a new webhook on a channel.</summary>
    /// <param name="channelId">The channel to host the webhook on.</param>
    /// <param name="name">The default name (1–80 chars).</param>
    /// <param name="avatar">Optional base64 image data URI for the default avatar.</param>
    IRestAction<IWebhook> Create(Snowflake channelId, string name, string? avatar = null);

    /// <summary>Lists the webhooks attached to a channel.</summary>
    IRestAction<IReadOnlyList<IWebhook>> ListForChannel(Snowflake channelId);
}
