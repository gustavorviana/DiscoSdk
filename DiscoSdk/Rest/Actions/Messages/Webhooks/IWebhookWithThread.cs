using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions.Messages.Webhooks;

public interface IWebhookWithThread<TSelf> where TSelf : IWebhookWithThread<TSelf>
{
    TSelf SetThread(Snowflake? threadId);
}
