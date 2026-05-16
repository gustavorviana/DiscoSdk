using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Mentions;
using DiscoSdk.Models.Messages.Pools;
using DiscoSdk.Hosting.Models.Requests.Messages;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Hosting.Rest.Actions.Messages;

/// <summary>
/// Base class for message building actions (create, edit, webhook).
/// Provides common validation and building functionality for all message operations.
/// </summary>
internal abstract class MessageBuilderAction<TSelf, TMessage> : RestAction<TMessage>, IMessageBuilderAction<TSelf, TMessage>
{
    protected string? _content;
    protected bool _suppressEmbeds;

    protected AllowedMentions? _allowedMentions;
    protected Poll? _poll;

    protected readonly List<Embed> _embeds = [];
    protected readonly List<IInteractionComponent> _components = [];
    protected readonly List<MessageFile> _attachments = [];

    private const long MaxRequestSizeBytes = 25 * 1024 * 1024; // 25 MiB
    private const int MaxEmbedCount = 10;
    private const int MaxComponentRows = 5;
    private const int MaxContentLength = 2000;

	public TSelf AddActionRow(params IInteractionComponent[] items)
	{
		ArgumentNullException.ThrowIfNull(items);

		if (items.Length == 0)
			throw new ArgumentException("At least one component must be provided.", nameof(items));

		if (items.Length > 5)
			throw new ArgumentException("Action row cannot contain more than 5 components.", nameof(items));

		if (_components.Count >= MaxComponentRows)
			throw new InvalidOperationException("Message cannot have more than 5 component rows.");

		// Wrap in MessageActionRowComponent (whose Components is IInteractionComponent[]) so
		// builder output — ButtonComponent / StringSelectComponent / etc. — can be passed
		// straight through. The old cast to the MessageComponent god-class only worked when
		// callers built components by hand; typed-builder output now lives alongside it.
		var actionRow = new MessageActionRowComponent
		{
			Components = items,
		};

		_components.Add(actionRow);
		return Self();
	}

	public TSelf AddActionRow(IInteractionComponentBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder);
		return AddActionRow(builder.Build());
	}

	/// <summary>
	/// Adds a top-level Components V2 component (TextDisplay, Section, MediaGallery, File,
	/// Separator, Container, etc.). Switches the message to the Components V2 layout by setting
	/// <see cref="MessageFlags.IsComponentV2"/> automatically on send. Mixing V2 components with
	/// plain <c>content</c> or <c>embeds</c> is rejected by Discord — clear those first.
	/// </summary>
	public TSelf AddComponent(IMessageComponent component)
	{
		ArgumentNullException.ThrowIfNull(component);
		_components.Add(component);
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
            Attachments = BuildAttachmentMetadataToCreate()
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
            Attachments = BuildAttachmentMetadataToCreate()
        };
    }

    protected MessageEditRequest BuildEditRequest(Message original)
    {
        var flags = BuildFlags();
        if (flags == MessageFlags.None)
            flags = original.Flags;

        return new MessageEditRequest
        {
            Content = _content ?? original.Content,
            Embeds = _embeds.Count > 0 ? [.. _embeds] : original.Embeds,
            Components = _components.Count > 0 ? [.. _components] : original.Components,
            AllowedMentions = _allowedMentions,
            Flags = flags,
            Attachments = BuildAttachmentMetadataToEdit(original)
        };
    }

    protected virtual MessageFlags BuildFlags()
    {
        var flags = MessageFlags.None;

        if (_suppressEmbeds)
            flags |= MessageFlags.SuppressEmbeds;

        if (HasComponentsV2())
            flags |= MessageFlags.IsComponentV2;

        return flags;
    }

    /// <summary>
    /// Returns true if any accumulated component is a Components V2 type (anything except
    /// <see cref="ComponentType.ActionRow"/>). When true, the <c>IS_COMPONENTS_V2</c> message
    /// flag must be set or Discord rejects the request.
    /// </summary>
    private bool HasComponentsV2()
    {
        foreach (var c in _components)
        {
            if (c.Type != ComponentType.ActionRow)
                return true;
        }
        return false;
    }

    protected MessageAttachmentMetadata[]? BuildAttachmentMetadataToEdit(Message original)
    {
        var originalAttachments = original.Attachments;
        var newAttachmentMetadata = BuildAttachmentMetadataToCreate();

        if (originalAttachments.Length == 0 && newAttachmentMetadata == null)
            return null;

        var combinedMetadata = new List<MessageAttachmentMetadata>();

        if (originalAttachments?.Length > 0)
            combinedMetadata.AddRange(originalAttachments
                .Select(att => new MessageAttachmentMetadata(att.Id)));

        if (newAttachmentMetadata != null)
            combinedMetadata.AddRange(newAttachmentMetadata);

        return [.. combinedMetadata];
    }

    protected virtual MessageAttachmentMetadata[]? BuildAttachmentMetadataToCreate()
    {
        if (_attachments.Count == 0)
            return null;

        return [.._attachments
            .Select((attachment, i) => new MessageAttachmentMetadata(i, attachment.FileName, attachment.Description))];
    }

    private TSelf Self()
    {
        return (TSelf)(object)this;
    }

    /// <summary>
    /// Validates message content for Discord API requirements.
    /// Checks for required fields, length limits, and invalid unicode characters.
    /// </summary>
    protected void ValidateMessageContent(Message? originalMessage)
    {
        var content = _content ?? originalMessage?.Content;
        List<Embed> embeds = _embeds.Count > 0 ? _embeds : originalMessage?.Embeds?.ToList() ?? [];
        var hasAttachment = _attachments.Count > 0 || (originalMessage != null && originalMessage.Attachments.Length > 0);
        // Components V2 messages replace the content+embeds slot entirely; their components carry
        // the visible payload (TextDisplay / Section / etc), so the message is valid without
        // any of the V1 fields.
        var hasComponentsV2 = HasComponentsV2();

        if (string.IsNullOrWhiteSpace(content) && embeds.Count == 0 && _poll == null && !hasAttachment && !hasComponentsV2)
            throw new InvalidOperationException("Message must have either content, at least one embed, a poll, or Components V2.");

        // Validate content length if present
        if (!string.IsNullOrEmpty(content) && content.Length > MaxContentLength)
            throw new ArgumentException($"Message content cannot exceed {MaxContentLength} characters.", nameof(content));

        // Discord rejects messages that mix Components V2 with content/embeds/poll/stickers — V2
        // is meant to *replace* those, not coexist. Surface a clear local error instead of a 400.
        if (hasComponentsV2)
        {
            if (!string.IsNullOrWhiteSpace(_content))
                throw new InvalidOperationException("Components V2 messages cannot also set Content. Move the text into a TextDisplayComponent.");
            if (_embeds.Count > 0)
                throw new InvalidOperationException("Components V2 messages cannot also include Embeds. Use Container/Section/TextDisplay instead.");
            if (_poll is not null)
                throw new InvalidOperationException("Components V2 messages cannot also include a Poll.");
        }
    }

    /// <summary>
    /// Validates embeds for Discord API requirements.
    /// Checks embed count limits and field constraints.
    /// </summary>
    protected void ValidateEmbeds()
    {
        if (_embeds.Count > MaxEmbedCount)
            throw new InvalidOperationException($"Message cannot have more than {MaxEmbedCount} embeds.");

        foreach (var embed in _embeds)
        {
            if (embed == null)
                continue;

            // Validate embed total character count (max 6000 across all embeds)
            var totalEmbedChars = _embeds.Sum(e => GetEmbedCharacterCount(e));
            if (totalEmbedChars > 6000)
                throw new InvalidOperationException("Total character count across all embeds cannot exceed 6000 characters.");

            // Validate individual embed field constraints
            if (embed.Title != null && embed.Title.Length > 256)
                throw new ArgumentException("Embed title cannot exceed 256 characters.");
            if (embed.Description != null && embed.Description.Length > 4096)
                throw new ArgumentException("Embed description cannot exceed 4096 characters.");
            if (embed.Footer?.Text != null && embed.Footer.Text.Length > 2048)
                throw new ArgumentException("Embed footer text cannot exceed 2048 characters.");
            if (embed.Author?.Name != null && embed.Author.Name.Length > 256)
                throw new ArgumentException("Embed author name cannot exceed 256 characters.");

            if (embed.Fields != null && embed.Fields.Length > 25)
                throw new ArgumentException("Embed cannot have more than 25 fields.");

            foreach (var field in embed.Fields ?? [])
            {
                if (field.Name.Length > 256)
                    throw new ArgumentException("Embed field name cannot exceed 256 characters.");
                if (field.Value.Length > 1024)
                    throw new ArgumentException("Embed field value cannot exceed 1024 characters.");
            }
        }
    }

    /// <summary>
    /// Validates message components for Discord API requirements.
    /// Checks component row limits.
    /// </summary>
    protected void ValidateComponents()
    {
        if (_components.Count > MaxComponentRows)
            throw new InvalidOperationException($"Message cannot have more than {MaxComponentRows} component rows.");

        foreach (var c in _components)
            ValidateComponentTree(c);
    }

    /// <summary>
    /// Recursive validation of a single component subtree. Enforces Discord's per-component limits
    /// (button label ≤ 80, select option label/value/description ≤ 100, select ≤ 25 options, etc.)
    /// so the caller gets a clear local exception instead of a cryptic 400 from the API.
    /// </summary>
    private static void ValidateComponentTree(IInteractionComponent c)
    {
        switch (c)
        {
            case ButtonComponent btn:
                if (btn.Label is { Length: > 80 })
                    throw new ArgumentException("Button label cannot exceed 80 characters.");
                if (btn.Style != ButtonStyle.Link && btn.Style != ButtonStyle.Premium && string.IsNullOrWhiteSpace(btn.CustomId))
                    throw new ArgumentException("Non-link, non-premium buttons require a custom_id.");
                if (btn.Style == ButtonStyle.Link && string.IsNullOrWhiteSpace(btn.Url))
                    throw new ArgumentException("Link buttons require a url.");
                break;

            case StringSelectComponent ss:
                if (ss.Options.Length > 25)
                    throw new ArgumentException("StringSelect cannot have more than 25 options.");
                if (ss.Options.Length < 1)
                    throw new ArgumentException("StringSelect must have at least one option.");
                foreach (var opt in ss.Options)
                    ValidateSelectOption(opt);
                if (ss.Placeholder is { Length: > 150 })
                    throw new ArgumentException("Select placeholder cannot exceed 150 characters.");
                break;

            case UserSelectComponent or RoleSelectComponent or ChannelSelectComponent or MentionableSelectComponent:
                // Entity selects share the same placeholder/min/max validation surface.
                var placeholder = c switch
                {
                    UserSelectComponent u => u.Placeholder,
                    RoleSelectComponent r => r.Placeholder,
                    ChannelSelectComponent ch => ch.Placeholder,
                    MentionableSelectComponent m => m.Placeholder,
                    _ => null,
                };
                if (placeholder is { Length: > 150 })
                    throw new ArgumentException("Select placeholder cannot exceed 150 characters.");
                break;

            case MessageActionRowComponent row:
                if (row.Components.Length > 5)
                    throw new ArgumentException("Action row cannot contain more than 5 components.");
                foreach (var inner in row.Components)
                    ValidateComponentTree(inner);
                break;

            case ContainerComponent container:
                foreach (var inner in container.Components)
                    ValidateComponentTree(inner);
                break;

            case SectionComponent section:
                if (section.Components.Length is < 1 or > 3)
                    throw new ArgumentException("Section requires 1–3 TextDisplay components.");
                if (section.Accessory is null)
                    throw new ArgumentException("Section requires an accessory (Button or Thumbnail).");
                ValidateComponentTree(section.Accessory);
                break;

            case MediaGalleryComponent gallery:
                if (gallery.Items.Length is < 1 or > 10)
                    throw new ArgumentException("MediaGallery requires 1–10 items.");
                break;

            case TextDisplayComponent td:
                if (string.IsNullOrEmpty(td.Content))
                    throw new ArgumentException("TextDisplay content is required.");
                if (td.Content.Length > 4000)
                    throw new ArgumentException("TextDisplay content cannot exceed 4000 characters.");
                break;

            // MessageComponent god-class — best-effort: validate label cap when type is Button.
            case MessageComponent mc when mc.Type == ComponentType.Button && mc.Label is { Length: > 80 }:
                throw new ArgumentException("Button label cannot exceed 80 characters.");

            // MessageComponent ActionRow god-class wrap — recurse into children.
            case MessageComponent mc when mc.Type == ComponentType.ActionRow && mc.Components is { } children:
                if (children.Length > 5)
                    throw new ArgumentException("Action row cannot contain more than 5 components.");
                foreach (var inner in children)
                    ValidateComponentTree(inner);
                break;
        }
    }

    private static void ValidateSelectOption(SelectOption opt)
    {
        if (string.IsNullOrWhiteSpace(opt.Label) || opt.Label.Length > 100)
            throw new ArgumentException("Select option label is required and must be ≤ 100 characters.");
        if (string.IsNullOrWhiteSpace(opt.Value) || opt.Value.Length > 100)
            throw new ArgumentException("Select option value is required and must be ≤ 100 characters.");
        if (opt.Description is { Length: > 100 })
            throw new ArgumentException("Select option description cannot exceed 100 characters.");
    }

    /// <summary>
    /// Validates request size does not exceed Discord's 25 MiB limit.
    /// </summary>
    protected void ValidateRequestSize()
    {
        // Estimate request size (content + embeds + attachments)
        long estimatedSize = 0;

        if (!string.IsNullOrEmpty(_content))
            estimatedSize += System.Text.Encoding.UTF8.GetByteCount(_content);

        foreach (var embed in _embeds)
            estimatedSize += EstimateEmbedSize(embed);

        foreach (var attachment in _attachments)
            estimatedSize += attachment.Buffer?.Length ?? 0;

        if (estimatedSize > MaxRequestSizeBytes)
            throw new InvalidOperationException($"Request size ({estimatedSize} bytes) exceeds maximum allowed size ({MaxRequestSizeBytes} bytes).");
    }

    /// <summary>
    /// Calculates total character count for an embed.
    /// </summary>
    private static int GetEmbedCharacterCount(Embed embed)
    {
        var count = 0;
        if (!string.IsNullOrEmpty(embed.Title)) count += embed.Title.Length;
        if (!string.IsNullOrEmpty(embed.Description)) count += embed.Description.Length;
        if (embed.Footer?.Text != null) count += embed.Footer.Text.Length;
        if (embed.Author?.Name != null) count += embed.Author.Name.Length;
        if (embed.Fields != null)
        {
            foreach (var field in embed.Fields)
            {
                count += field.Name.Length + field.Value.Length;
            }
        }
        return count;
    }

    /// <summary>
    /// Estimates byte size of an embed for request size validation.
    /// </summary>
    private static long EstimateEmbedSize(Embed embed)
    {
        long size = 0;
        if (embed.Title != null) size += System.Text.Encoding.UTF8.GetByteCount(embed.Title);
        if (embed.Description != null) size += System.Text.Encoding.UTF8.GetByteCount(embed.Description);
        if (embed.Footer?.Text != null) size += System.Text.Encoding.UTF8.GetByteCount(embed.Footer.Text);
        if (embed.Author?.Name != null) size += System.Text.Encoding.UTF8.GetByteCount(embed.Author.Name);
        if (embed.Fields != null)
        {
            foreach (var field in embed.Fields)
            {
                size += System.Text.Encoding.UTF8.GetByteCount(field.Name);
                size += System.Text.Encoding.UTF8.GetByteCount(field.Value);
            }
        }
        return size;
    }
}