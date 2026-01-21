using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions.Messages.Webhooks;

namespace DiscoSdk.Hosting.Rest.Actions.Messages.Webhooks;

internal class WebhookEditMessageRestAction(WebhookIdentity identity, WebhookMessageClient client, Snowflake messageId)
    : MessageBuilderAction<IWebhookEditMessageRestAction, IWebhookMessage>, IWebhookEditMessageRestAction
{
    private Snowflake? _threadId;

    public IWebhookEditMessageRestAction SetThread(Snowflake? threadId)
    {
        _threadId = threadId;
        return this;
    }

    public override async Task<IWebhookMessage> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var resultMessage = await client.EditAsync(identity,
            messageId,
            BuildEditRequest(),
            _attachments,
            _threadId,
            cancellationToken);

        return new WebhookMessageWrapper(identity, client, resultMessage);
    }
}