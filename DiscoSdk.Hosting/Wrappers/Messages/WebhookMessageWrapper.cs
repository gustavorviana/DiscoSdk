using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Actions.Messages.Webhooks;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages.Webhooks;

namespace DiscoSdk.Hosting.Wrappers.Messages;

internal class WebhookMessageWrapper(WebhookIdentity id, WebhookMessageClient client, Message message) : MessageBaseWrapper(message), IWebhookMessage
{
    public Snowflake ChannelId => Message.ChannelId;

    public IRestAction Delete(Snowflake? threadId = null)
    {
        return RestAction.Create(async cancellationToken => await client.DeleteAsync(id, Message.Id, threadId, cancellationToken));
    }

    public IWebhookEditMessageRestAction Edit()
    {
        return new WebhookEditMessageRestAction(id, client, Message.Id);
    }
}