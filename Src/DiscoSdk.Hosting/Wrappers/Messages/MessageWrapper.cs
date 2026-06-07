using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Actions.Messages;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Hosting.Wrappers.Messages;

/// <summary>
/// Wrapper that implements <see cref="IMessage"/> for a <see cref="Message"/> instance.
/// </summary>
internal class MessageWrapper : MessageBaseWrapper, IMessage
{
    private readonly InteractionHandle? _interactionHandle;
    private readonly DiscordClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageWrapper"/> class.
    /// </summary>
    /// <param name="message">The message instance to wrap.</param>
    /// <param name="client">The Discord client for performing operations.</param>
    public MessageWrapper(DiscordClient client, ITextBasedChannel channel, Message message, InteractionHandle? interactionHandle)
        : base(message)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionHandle = interactionHandle;

        Reactions = Message.Reactions?.Select(r => new ReactionWrapper(r, this, _client))?.ToArray() ?? [];
        Author = new UserWrapper(client, message.Author);
        Mentions = message.Mentions?.Select(x => new UserMentionWrapper(client, x, Guild))?.ToArray() ?? [];
    }

    public Snowflake Id => Message.Id;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => Message.Id.CreatedAt;

    public IUser Author { get; }
    public override string Content
    {
        get
        {
            WarnIfMessageContentMissing(MessageContentField.Content);
            return Message.Content ?? string.Empty;
        }
    }
    public override DiscoSdk.Models.Messages.Embeds.Embed[] Embeds
    {
        get
        {
            WarnIfMessageContentMissing(MessageContentField.Embeds);
            return Message.Embeds ?? [];
        }
    }
    public override DiscoSdk.Models.Messages.Attachment[] Attachments
    {
        get
        {
            WarnIfMessageContentMissing(MessageContentField.Attachments);
            return Message.Attachments ?? [];
        }
    }
    public override DiscoSdk.Models.Messages.Pools.Poll? Poll
    {
        get
        {
            WarnIfMessageContentMissing(MessageContentField.Poll);
            return Message.Pool;
        }
    }
    public IInteractionComponent[]? Components
    {
        get
        {
            WarnIfMessageContentMissing(MessageContentField.Components);
            return Message.Components;
        }
    }
    public IReaction[] Reactions { get; }
    private IReadOnlyList<IMessageSnapshot>? _messageSnapshots;
    public IReadOnlyList<IMessageSnapshot> MessageSnapshots => _messageSnapshots ??=
        Message.MessageSnapshots is { Length: > 0 } snapshots
            ? [.. snapshots.Select(s => new MessageSnapshotWrapper(_client, s, Guild))]
            : [];
    public string Timestamp => Message.Timestamp;
    public string? EditedTimestamp => Message.EditedTimestamp;

    public ITextBasedChannel Channel { get; }

    public IGuild? Guild
    {
        get
        {
            if (Channel is IGuildChannel guildChannel)
                return guildChannel.Guild;

            return null;
        }
    }

    public IUserMention[] Mentions { get; }

    // Operations with Builders
    public IEditMessageRestAction Edit()
    {
        if (Message.Flags.HasFlag(MessageFlags.Ephemeral) && _interactionHandle == null)
            throw EphemeralMessageException.Operation("edit");

        // Only allow editing messages from the bot itself
        if (!IsBotMessage())
            throw InsufficientPermissionException.Operation("MANAGE_MESSAGES", "edit messages from other users");

        return new EditMessageRestAction(_client, Channel, Message, _interactionHandle);
    }

    public ISendMessageRestAction ToBuilder(ITextBasedChannel? target = null)
    {
        // Reads against Content / Embeds / Components / Poll flow through the guarded properties
        // so the MessageContent warn-once fires for callers cloning a message they only have
        // header access to (the gated fields will come back empty in that case and the fork still
        // produces something Discord accepts).
        var builder = new SendMessageRestAction(_client, null, target ?? Channel, content: Content);

        if (Embeds is { Length: > 0 })
        {
            builder.SetEmbeds([.. Embeds
                .Select(EmbedBuilder.From)
                .Select(b => b.Build())]);
        }

        if (Components is { Length: > 0 })
        {
            foreach (var component in Components)
            {
                if (component is IMessageComponent mc)
                    builder.AddComponent(mc);
            }
        }

        if (Poll != null)
            builder.SetPoll(Poll);

        if (Message.Tts)
            builder.SetTts(true);

        if (Message.Flags.HasFlag(MessageFlags.SuppressNotifications))
            builder.SetSuppressNotifications(true);

        if (Message.Flags.HasFlag(MessageFlags.SuppressEmbeds))
            builder.SetSuppressEmbeds(true);

        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            builder.SetEphemeral(true);

        // StickerItems is the always-populated id+name+format projection; the Stickers field
        // (full sticker objects) is rare and not needed here — Discord re-resolves from ids.
        if (Message.StickerItems is { Length: > 0 } stickers)
            builder.SetStickers(stickers.Select(s => s.Id));

        // Preserve reply / forward references. The Type field decides whether Discord interprets
        // the new send as a reply or a forward; default replies omit Type on the wire.
        if (Message.MessageReference is { } reference && !string.IsNullOrEmpty(reference.MessageId))
        {
            builder.SetMessageReference(
                reference.Type ?? MessageReferenceType.Default,
                reference.MessageId,
                reference.ChannelId,
                reference.GuildId,
                reference.FailIfNotExists);
        }

        return builder;
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        return new SendMessageRestAction(_client, null, Channel, content)
            .SetMessageReference(Message.Id.ToString(), Message.ChannelId.ToString(), Message.GuildId?.ToString());
    }

    public ISendMessageRestAction ForwardTo(ITextBasedChannel target)
    {
        ArgumentNullException.ThrowIfNull(target);

        // Forwards carry no content/embeds — Discord ignores those when type=Forward — so the
        // builder is created with null content and callers should not set content/embeds on it.
        var action = new SendMessageRestAction(_client, null, target, content: null);
        action.SetMessageReference(MessageReferenceType.Forward,
            Message.Id.ToString(),
            Message.ChannelId.ToString(),
            Message.GuildId?.ToString());
        return action;
    }

    // Direct Operations
    /// <inheritdoc />
    public IReasonedRestAction Delete()
    {
        if (Message.Flags.HasFlag(MessageFlags.Ephemeral) && _interactionHandle == null)
            throw EphemeralMessageException.Operation("delete");

        return new ReasonedRestAction((reason, ct) =>
        {
            if (_interactionHandle != null)
                return new WebhookMessageClient(_client.HttpClient).DeleteOriginalResponseAsync(_interactionHandle.WithAppId(_client.ApplicationId), ct);

            return _client.MessageClient.DeleteAsync(Message.ChannelId, Message.Id, reason, ct);
        });
    }

    IRestAction IDeletable.Delete() => Delete();

    public IRestAction<IMessage> Crosspost()
    {
        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("crosspost");

        return RestAction<IMessage>.Create(async cancellationToken =>
        {
            var message = await _client.MessageClient.CrosspostAsync(Message.ChannelId, Message.Id, cancellationToken);
            return new MessageWrapper(_client, Channel, message, null);
        });
    }

    public IRestAction AddReaction(string emoji)
    {
        ValidateReactionIntent("add reactions to");

        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("add reactions to");

        return RestAction.Create(cancellationToken =>
            _client.MessageClient.AddReactionAsync(Message.ChannelId, Message.Id, emoji, cancellationToken));
    }

    public IGetReactionsAction GetReactions(string emoji)
    {
        ValidateReactionIntent("get reactions from");

        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("get reactions from");

        return new GetReactionsAction(_client, Message.ChannelId, Message.Id, emoji);
    }

    public IRestAction DeleteAllReactionsForEmoji(string emoji)
    {
        ValidateReactionIntent("delete reactions from");

        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("delete reactions from");

        return RestAction.Create(cancellationToken =>
            _client.MessageClient.DeleteAllReactionsForEmojiAsync(Message.ChannelId, Message.Id, emoji, cancellationToken));
    }

    public IRestAction DeleteAllReactions()
    {
        ValidateReactionIntent("delete all reactions from");

        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("delete all reactions from");

        return RestAction.Create(cancellationToken =>
            _client.MessageClient.DeleteAllReactionsAsync(Message.ChannelId, Message.Id, cancellationToken));
    }

    public IRestAction Pin()
    {
        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("pin");

        return RestAction.Create(cancellationToken =>
            _client.MessageClient.PinAsync(Message.ChannelId, Message.Id, cancellationToken));
    }

    public IRestAction Unpin()
    {
        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("unpin");

        return RestAction.Create(cancellationToken =>
            _client.MessageClient.UnpinAsync(Message.ChannelId, Message.Id, cancellationToken));
    }

    /// <summary>
    /// Asserts that the appropriate reaction intent (<see cref="DiscordIntent.GuildMessageReactions"/>
    /// or <see cref="DiscordIntent.DirectMessageReactions"/>) is enabled for this message's scope.
    /// </summary>
    private void ValidateReactionIntent(string operation)
    {
        var required = Message.GuildId.HasValue
            ? DiscordIntent.GuildMessageReactions
            : DiscordIntent.DirectMessageReactions;

        IntentGuard.Require(_client, required, operation);
    }

    public Channel GetChannel()
    {
        throw new NotSupportedException("This operation is not yet supported.");
    }

    /// <summary>
    /// Checks if the message was sent by the bot.
    /// </summary>
    /// <returns>True if the message author is the bot, false otherwise.</returns>
    private bool IsBotMessage()
    {
        if (!TryGetBotUserId(out var botId))
            return false;

        return Message.Author.UserId == botId;
    }

    /// <summary>
    /// Parses the bot's user id from <see cref="DiscordClient.BotUser"/> into a <see cref="Snowflake"/>.
    /// </summary>
    private bool TryGetBotUserId(out Snowflake botId)
    {
        botId = default;
        if (string.IsNullOrEmpty(_client.BotUser?.Id))
            return false;

        return Snowflake.TryParse(_client.BotUser.Id, out botId);
    }

    /// <summary>
    /// Emits a one-shot warning (per field, per process) when a content-gated field is read
    /// without the privileged <see cref="DiscordIntent.MessageContent"/> intent and outside
    /// Discord's exemption list (bot is author, bot is mentioned, channel is DM / group DM).
    /// The accessor itself returns the underlying value — empty for non-exempt messages without
    /// the intent — so callers using <c>?.</c>, LINQ, logging, or serialization don't blow up
    /// on a property getter. Warn-once-per-field prevents log spam on hot paths while giving
    /// the operator a single grep target the first time the misconfig is hit.
    /// </summary>
    private void WarnIfMessageContentMissing(MessageContentField field)
    {
        // Intent on — nothing to warn about.
        if (_client.Intents.HasFlag(DiscordIntent.MessageContent))
            return;

        // Bot is the author — Discord populates content on its own messages.
        if (IsBotMessage())
            return;

        // DM / group DM — Discord exempts the entire channel category from MessageContent.
        if (Channel.Type is ChannelType.Dm or ChannelType.GroupDm)
            return;

        // Bot is mentioned — Discord populates content on @-mention.
        if (TryGetBotUserId(out var botId)
            && Message.Mentions is { Length: > 0 } mentions
            && mentions.Any(m => m.UserId == botId))
            return;

        MessageContentWarnTracker.WarnOnce(_client.Logger, field);
    }
}