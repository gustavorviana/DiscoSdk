using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions.Messages.Webhooks;

namespace DiscoSdk.Hosting.Rest.Actions.Messages.Webhooks;

internal class WebhookEditMessageRestAction
    : MessageBuilderAction<IWebhookEditMessageRestAction, IWebhookMessage>, IWebhookEditMessageRestAction
{
    private readonly WebhookMessageClient _client;
    private readonly WebhookIdentity _identity;
    private readonly Snowflake _messageId;
    private Snowflake? _threadId;
    private Message? _message;

    public WebhookEditMessageRestAction(WebhookIdentity identity, WebhookMessageClient client, Message message)
    {
        _messageId = message.Id;
        _message = message;
        _identity = identity;
        _client = client;
    }

    public WebhookEditMessageRestAction(WebhookIdentity identity, WebhookMessageClient client, Snowflake messageId)
    {
        _messageId = messageId;
        _identity = identity;
        _client = client;
    }

    public IWebhookEditMessageRestAction SetThread(Snowflake? threadId)
    {
        _threadId = threadId;
        return this;
    }

    public override async Task<IWebhookMessage> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var originalMessage = await GetOriginalMessageAsync(cancellationToken);

        ValidateMessageContent(originalMessage);
        ValidateEmbeds();
        ValidateComponents();
        ValidateRequestSize();

        var resultMessage = await _client.EditAsync(_identity,
            _messageId,
            BuildEditRequest(originalMessage),
            _attachments,
            _threadId,
            cancellationToken);

        return new WebhookMessageWrapper(_identity, _client, resultMessage);
    }

    private async Task<Message> GetOriginalMessageAsync(CancellationToken cancellationToken)
    {
        if (_message is not null)
            return _message;

        return _message = await _client.GetAsync(_identity, _messageId, _threadId, cancellationToken);
    }
}