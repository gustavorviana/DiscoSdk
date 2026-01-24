using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
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
    /// Retrieves a message previously sent by this webhook.
    /// </summary>
    /// <param name="messageId">
    /// The ID of the message to retrieve.
    /// </param>
    /// <param name="threadId">
    /// Optional thread ID. When provided, the request will be executed in the context of the specified thread.
    /// </param>
    /// <returns>
    /// A REST action that returns the requested webhook message information.
    /// </returns>
    IRestAction<IWebhookMessage?> GetMessageById(Snowflake messageId, Snowflake? threadId = null);

    /// <summary>
    /// Creates a builder for sending a message using this webhook.
    /// </summary>
    /// <param name="threadId">
    /// Optional thread ID. When provided, the message will be sent in the specified thread.
    /// </param>
    IWebhookSendMessageRestAction Send(Snowflake? threadId = null);

    /// <summary>
    /// Creates a builder for editing a message previously sent by this webhook.
    /// </summary>
    /// <param name="messageId">
    /// The ID of the message to edit.
    /// </param>
    IWebhookEditMessageRestAction EditMessage(Snowflake messageId);

    /// <summary>
    /// Deletes a message previously sent by this webhook.
    /// </summary>
    /// <param name="messageId">
    /// The ID of the message to delete.
    /// </param>
    /// <param name="threadId">
    /// Optional thread ID. When provided, the deletion will be executed in the context of the specified thread.
    /// </param>
    IRestAction DeleteMessage(Snowflake messageId, Snowflake? threadId = null);

}