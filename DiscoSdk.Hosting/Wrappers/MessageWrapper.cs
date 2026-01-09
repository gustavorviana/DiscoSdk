using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Events;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IMessage"/> for a <see cref="Message"/> instance.
/// </summary>
internal class MessageWrapper : IMessage
{
    private readonly InteractionHandle? _interactionHandle;
    private readonly DiscordClient _client;
    private readonly Message _message;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageWrapper"/> class.
    /// </summary>
    /// <param name="message">The message instance to wrap.</param>
    /// <param name="client">The Discord client for performing operations.</param>
    public MessageWrapper(ITextBasedChannel channel, Message message, DiscordClient client, InteractionHandle? interactionHandle)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionHandle = interactionHandle;

        Reactions = _message.Reactions?.Select(r => new ReactionWrapper(r, this, _client))?.ToArray() ?? [];
    }

    // Visual Properties
    public DiscordId Id => _message.Id;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => _message.Id.CreatedAt;

    public User Author => _message.Author;
    public string Content
    {
        get
        {
            ValidateIntent(DiscordIntent.MessageContent, "access message content");
            return _message.Content;
        }
    }
    public Embed[] Embeds => _message.Embeds;
    public MessageComponent[]? Components => _message.Components;
    public Attachment[] Attachments => _message.Attachments;
    public IReaction[] Reactions { get; }
    public string Timestamp => _message.Timestamp;
    public string? EditedTimestamp => _message.EditedTimestamp;
    public bool Pinned => _message.Pinned;
    public MessageFlags Flags => _message.Flags;
    public MessageType Type => _message.Type;

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

    // Operations with Builders
    public IEditMessageRestAction Edit()
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral) && _interactionHandle == null)
            throw EphemeralMessageException.Operation("edit");

        // Only allow editing messages from the bot itself
        if (!IsBotMessage())
            throw InsufficientPermissionException.Operation("MANAGE_MESSAGES", "edit messages from other users");

        return new EditMessageRestAction(_client, Channel, _message.Id, _interactionHandle);
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        return new SendMessageRestAction(_client, null, Channel, content)
            .SetMessageReference(_message.Id.ToString(), _message.ChannelId.ToString(), _message.GuildId?.ToString());
    }

    // Direct Operations
    /// <inheritdoc />
    public IRestAction Delete()
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral) && _interactionHandle == null)
            throw EphemeralMessageException.Operation("delete");

        return new RestAction(cancellationToken =>
        {
            if (_interactionHandle != null)
                return _client.InteractionClient.DeleteOriginalResponseAsync(_interactionHandle, cancellationToken);

            return _client.MessageClient.DeleteAsync(_message.ChannelId, _message.Id, cancellationToken);
        });
    }

    public IRestAction<IMessage> Crosspost()
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("crosspost");

        return RestAction<IMessage>.Create(async cancellationToken =>
        {
            var message = await _client.MessageClient.CrosspostAsync(_message.ChannelId, _message.Id, cancellationToken);
            return new MessageWrapper(Channel, message, _client, null);
        });
    }

    public IRestAction AddReaction(string emoji)
    {
        ValidateReactionIntent("add reactions to");

        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("add reactions to");

        return new RestAction(cancellationToken =>
            _client.MessageClient.AddReactionAsync(_message.ChannelId, _message.Id, emoji, cancellationToken));
    }

    public IRestAction<User[]> GetReactions(string emoji, string? after = null, int? limit = null)
    {
        ValidateReactionIntent("get reactions from");

        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("get reactions from");

        return RestAction<User[]>.Create(cancellationToken =>
            _client.MessageClient.GetReactionsAsync(_message.ChannelId, _message.Id, emoji, after, limit, cancellationToken));
    }

    public IRestAction DeleteAllReactionsForEmoji(string emoji)
    {
        ValidateReactionIntent("delete reactions from");

        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("delete reactions from");

        return new RestAction(cancellationToken =>
            _client.MessageClient.DeleteAllReactionsForEmojiAsync(_message.ChannelId, _message.Id, emoji, cancellationToken));
    }

    public IRestAction DeleteAllReactions()
    {
        ValidateReactionIntent("delete all reactions from");

        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("delete all reactions from");

        return new RestAction(cancellationToken =>
            _client.MessageClient.DeleteAllReactionsAsync(_message.ChannelId, _message.Id, cancellationToken));
    }

    public IRestAction Pin()
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("pin");

        return new RestAction(cancellationToken =>
            _client.MessageClient.PinAsync(_message.ChannelId, _message.Id, cancellationToken));
    }

    public IRestAction Unpin()
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("unpin");

        return new RestAction(cancellationToken =>
            _client.MessageClient.UnpinAsync(_message.ChannelId, _message.Id, cancellationToken));
    }

    /// <summary>
    /// Validates that the required intent is enabled.
    /// </summary>
    /// <param name="requiredIntent">The intent that is required.</param>
    /// <param name="operation">The operation being performed.</param>
    /// <exception cref="MissingIntentException">Thrown when the required intent is not enabled.</exception>
    private void ValidateIntent(DiscordIntent requiredIntent, string operation)
    {
        if (!_client.Intents.HasFlag(requiredIntent))
            throw new MissingIntentException(requiredIntent, operation);
    }

    /// <summary>
    /// Validates that the appropriate reaction intent is enabled based on whether the message is in a guild or DM.
    /// </summary>
    /// <param name="operation">The operation being performed.</param>
    /// <exception cref="MissingIntentException">Thrown when the required intent is not enabled.</exception>
    private void ValidateReactionIntent(string operation)
    {
        ValidateIntent(GetCurrentReactionIntent(), operation);
    }

    private DiscordIntent GetCurrentReactionIntent()
    {
        return _message.GuildId.HasValue
                    ? DiscordIntent.GuildMessageReactions
                    : DiscordIntent.DirectMessageReactions;
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
        if (string.IsNullOrEmpty(_client.User?.Id))
            return false;

        if (!DiscordId.TryParse(_client.User.Id, out var botId))
            return false;

        return _message.Author.Id == botId;
    }
}