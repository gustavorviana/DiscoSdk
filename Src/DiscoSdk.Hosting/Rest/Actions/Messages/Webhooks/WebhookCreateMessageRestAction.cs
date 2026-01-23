using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions.Messages.Webhooks;

namespace DiscoSdk.Hosting.Rest.Actions.Messages.Webhooks;

internal class WebhookCreateMessageRestAction(WebhookIdentity identity, WebhookMessageClient client)
    : MessageBuilderAction<IWebhookSendMessageRestAction, IWebhookMessage?>, IWebhookSendMessageRestAction
{
    private bool? _tts;
    private bool _wait;
    private string? _username;
    private string? _avatarUrl;
    private string? _threadName;
    private bool _suppressNotifications;
    private Snowflake? _threadId;

    private readonly List<ulong> _appliedTags = [];

    public IWebhookSendMessageRestAction Wait(bool wait = true)
    {
        _wait = wait;
        return this;
    }

    public IWebhookSendMessageRestAction SetAppliedTags(params ulong[] tagIds)
    {
        _appliedTags.Clear();

        if (tagIds != null && tagIds.Length > 0)
            _appliedTags.AddRange(tagIds);

        return this;
    }

    public IWebhookSendMessageRestAction SetAvatarUrl(string? avatarUrl)
    {
        _avatarUrl = avatarUrl;
        return this;
    }

    public IWebhookSendMessageRestAction SetSuppressNotifications(bool suppress = true)
    {
        _suppressNotifications = suppress;
        return this;
    }

    public IWebhookSendMessageRestAction SetThreadName(string? threadName)
    {
        _threadName = threadName;
        return this;
    }

    public IWebhookSendMessageRestAction SetTts(bool tts = true)
    {
        _tts = tts;
        return this;
    }

    public IWebhookSendMessageRestAction SetUsername(string? username)
    {
        _username = username;
        return this;
    }

    public IWebhookSendMessageRestAction SetThread(Snowflake? threadId)
    {
        _threadId = threadId;
        return this;
    }

    public override async Task<IWebhookMessage?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var flags = base.BuildFlags();

        if (_suppressNotifications)
            flags |= MessageFlags.SuppressNotifications;

        var obj = new ExecuteWebhookRequest
        {
            Content = _content,
            Tts = _tts,
            Username = _username,
            AvatarUrl = _avatarUrl,
            ThreadName = _threadName,
            AllowedMentions = _allowedMentions,
            Embeds = _embeds.Count == 0 ? null : _embeds.ToArray(),
            Components = _components.Count == 0 ? null : [.. _components],
            Poll = _poll,
            AppliedTags = _appliedTags.Count == 0 ? null : [.. _appliedTags],
            Attachments = BuildAttachmentMetadata(),
            Flags = flags
        };

        var message = await client.ExecuteAsync(identity, obj, _attachments, _wait, _threadId, cancellationToken);
        return _wait && message is not null ? new WebhookMessageWrapper(identity, client, message) : null;
    }
}