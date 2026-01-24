using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions.Messages.Webhooks;

/// <summary>
/// Represents a REST action builder for sending a message using a webhook.
/// </summary>
/// <remarks>
/// When <see cref="Wait(bool)"/> is enabled, the Discord API will return the created message,
/// allowing it to be retrieved and represented as an <see cref="IWebhookMessage"/> instance.
/// When disabled, the message is sent without waiting for a response body.
/// </remarks>
public interface IWebhookSendMessageRestAction
    : ICreateMessageBuilderBaseAction<IWebhookSendMessageRestAction, IWebhookMessage?>,
      IWebhookWithThread<IWebhookSendMessageRestAction>
{
    /// <summary>
    /// Sets whether the request should wait for the message to be created.
    /// </summary>
    /// <param name="wait">
    /// When <c>true</c>, the Discord API will return the created message in the response.
    /// When <c>false</c>, the message is sent without returning the created message.
    /// </param>
    /// <returns>
    /// The current <see cref="IWebhookSendMessageRestAction"/> instance.
    /// </returns>
    IWebhookSendMessageRestAction Wait(bool wait = true);

    /// <summary>
    /// Overrides the username used by the webhook for this message.
    /// </summary>
    /// <param name="username">
    /// The username to display as the message author.
    /// </param>
    /// <returns>
    /// The current <see cref="IWebhookSendMessageRestAction"/> instance.
    /// </returns>
    IWebhookSendMessageRestAction SetUsername(string? username);

    /// <summary>
    /// Overrides the avatar URL used by the webhook for this message.
    /// </summary>
    /// <param name="avatarUrl">
    /// The avatar URL to display as the message author.
    /// </param>
    /// <returns>
    /// The current <see cref="IWebhookSendMessageRestAction"/> instance.
    /// </returns>
    IWebhookSendMessageRestAction SetAvatarUrl(string? avatarUrl);

    /// <summary>
    /// Sets the name of the thread to be created when sending this message.
    /// </summary>
    /// <param name="threadName">
    /// The name of the thread to create.
    /// </param>
    /// <returns>
    /// The current <see cref="IWebhookSendMessageRestAction"/> instance.
    /// </returns>
    IWebhookSendMessageRestAction SetThreadName(string? threadName);

    /// <summary>
    /// Sets the applied tag IDs when sending a message in a forum channel.
    /// </summary>
    /// <param name="tagIds">
    /// The IDs of the tags to apply.
    /// </param>
    /// <returns>
    /// The current <see cref="IWebhookSendMessageRestAction"/> instance.
    /// </returns>
    IWebhookSendMessageRestAction SetAppliedTags(params ulong[] tagIds);
}