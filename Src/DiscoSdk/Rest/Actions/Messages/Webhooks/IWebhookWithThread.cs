using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions.Messages.Webhooks;

/// <summary>
/// Defines support for executing a webhook action within the context of a thread.
/// </summary>
/// <typeparam name="TSelf">
/// The concrete type implementing this interface, used to preserve fluent chaining.
/// </typeparam>
public interface IWebhookWithThread<TSelf>
    where TSelf : IWebhookWithThread<TSelf>
{
    /// <summary>
    /// Sets the thread in which the webhook action will be executed.
    /// </summary>
    /// <param name="threadId">
    /// The ID of the target thread.
    /// When <c>null</c>, the webhook action will be executed in the parent channel.
    /// </param>
    /// <returns>
    /// The current instance to allow fluent chaining.
    /// </returns>
    TSelf SetThread(Snowflake? threadId);
}