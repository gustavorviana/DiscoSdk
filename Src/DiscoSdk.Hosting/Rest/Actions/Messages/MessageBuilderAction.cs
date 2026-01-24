using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Mentions;
using DiscoSdk.Models.Messages.Pools;
using DiscoSdk.Models.Requests.Messages;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Hosting.Rest.Actions.Messages;

internal abstract class MessageBuilderAction<TSelf, TMessage> : RestAction<TMessage>, IMessageBuilderAction<TSelf, TMessage>
{
    protected string? _content;
    protected bool _suppressEmbeds;

    protected AllowedMentions? _allowedMentions;
    protected Poll? _poll;

    protected readonly List<Embed> _embeds = [];
    protected readonly List<MessageComponent> _components = [];
    protected readonly List<MessageFile> _attachments = [];

    public TSelf AddActionRow(params MessageComponent[] items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Length == 0)
            throw new ArgumentException("At least one component must be provided.", nameof(items));

        if (_components.Count >= 5)
            throw new InvalidOperationException("Message cannot have more than 5 component rows.");

        // Create an ActionRow containing the items
        var actionRow = new MessageComponent
        {
            Type = ComponentType.ActionRow,
            Components = [.. items]
        };

        _components.Add(actionRow);
        return Self();
    }

    public TSelf AttachFiles(params MessageFile[] files)
    {
        ArgumentNullException.ThrowIfNull(files);

        foreach (var file in files)
        {
            if (file == null)
                continue;

            _attachments.Add(file);
        }

        return Self();
    }

    public TSelf AttachFile(MessageFile file)
    {
        _attachments.Add(file);
        return Self();
    }

    public TSelf ClearAttachments()
    {
        _attachments.Clear();
        return Self();
    }

    public TSelf ClearComponents()
    {
        _components.Clear();
        return Self();
    }

    public TSelf SetAllowedMentions(MentionBuilder builder)
    {
        _allowedMentions = builder.BuildAllowedMentions();
        return Self();
    }

    public TSelf SetContent(string? content)
    {
        if (content != null && content.Length > 2000)
            throw new ArgumentException("Message content cannot exceed 2000 characters.", nameof(content));

        _content = content;
        return Self();
    }

    public TSelf SetContent(MessageTextBuilder builder)
    {
        _allowedMentions = builder.BuildAllowedMentions();
        return SetContent(builder.ToString());
    }

    public TSelf SetEmbeds(params Embed[] embeds)
    {
        _embeds.Clear();

        if (embeds != null && embeds.Length > 0)
            _embeds.AddRange(embeds);

        return Self();
    }

    public TSelf SetPoll(Poll? poll)
    {
        _poll = poll;
        return Self();
    }

    public TSelf SetSuppressEmbeds(bool suppress = true)
    {
        _suppressEmbeds = suppress;
        return Self();
    }

    protected virtual ExecuteWebhookRequest BuildWebhookCreateRequest()
    {
        return new ExecuteWebhookRequest
        {
            Content = _content,
            Embeds = _embeds.Count > 0 ? [.. _embeds] : null,
            Components = _components.Count > 0 ? [.. _components] : null,
            AllowedMentions = _allowedMentions,
            Flags = BuildFlags(),
            Poll = _poll,
            Attachments = BuildAttachmentMetadata()
        };
    }

    protected virtual MessageCreateRequest BuildCreateRequest()
    {
        return new MessageCreateRequest
        {
            Content = _content,
            Embeds = _embeds.Count > 0 ? [.. _embeds] : null,
            Components = _components.Count > 0 ? [.. _components] : null,
            AllowedMentions = _allowedMentions,
            Flags = BuildFlags(),
            Poll = _poll,
            Attachments = BuildAttachmentMetadata()
        };
    }

    protected MessageEditRequest BuildEditRequest()
    {
        return new MessageEditRequest
        {
            Content = _content,
            Embeds = _embeds.Count > 0 ? [.. _embeds] : null,
            Components = _components.Count > 0 ? [.. _components] : null,
            AllowedMentions = _allowedMentions,
            Flags = BuildFlags(),
            Poll = _poll,
            Attachments = BuildAttachmentMetadata()
        };
    }

    protected virtual MessageFlags BuildFlags()
    {
        var flags = MessageFlags.None;

        if (_suppressEmbeds)
            flags |= MessageFlags.SuppressEmbeds;

        return flags;
    }

    protected MessageAttachmentMetadata[]? BuildAttachmentMetadata()
    {
        if (_attachments.Count == 0)
            return null;

        var list = new MessageAttachmentMetadata[_attachments.Count];

        for (var i = 0; i < _attachments.Count; i++)
        {
            var attachment = _attachments[i];
            list[i] = new MessageAttachmentMetadata(i, attachment.FileName, attachment.Description);
        }

        return list;
    }

    private TSelf Self()
    {
        return (TSelf)(object)this;
    }
}