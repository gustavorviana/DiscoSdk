using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages.Webhooks;

namespace DiscoSdk.Models.Messages;

public interface IWebhookMessage : IMessageBase
{
    Snowflake ChannelId { get; }

    /// <summary>
    /// Creates a builder for editing this message.
    /// </summary>
    /// <returns>An <see cref="IWebhookEditMessageRestAction"/> instance for editing the message.</returns>
    IWebhookEditMessageRestAction Edit();

    IRestAction Delete(Snowflake? threadId = null);
}