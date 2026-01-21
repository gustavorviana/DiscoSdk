using DiscoSdk.Models;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages;
using DiscoSdk.Rest.Actions.Messages.Webhooks;

namespace DiscoSdk;

/// <summary>
/// Webhook-only Discord REST client.
/// </summary>
/// <remarks>
/// This interface is intentionally limited to what Discord Webhook REST endpoints support:
/// - Get webhook info (by id/token)
/// - Execute webhook (send message)
/// - Edit webhook message
/// - Delete webhook message
/// - Optional thread targeting (thread_id)
/// - Optional wait parameter (return created message)
/// - Optional username/avatar overrides (per request)
/// </remarks>
public interface IDiscordWebhookClient : IWebhookInfo
{
    /// <summary>
    /// Underlying REST client used to call Discord Webhook endpoints.
    /// </summary>
    IDiscordRestClient HttpClient { get; }

    /// <summary>
    /// Webhook token (secret).
    /// </summary>
    string Token { get; }

    /// <summary>
    /// Creates a builder for executing this webhook (send message).
    /// </summary>
    IWebhookSendMessageRestAction Send(Snowflake? threadId = null);

    /// <summary>
    /// Creates a builder for editing a message previously sent by this webhook
    /// </summary>
    IWebhookEditMessageRestAction EditMessage(Snowflake messageId);

    /// <summary>
    /// Deletes a message previously sent by this webhook
    /// </summary>
    IRestAction DeleteMessage(Snowflake messageId, Snowflake? threadId = null);
}