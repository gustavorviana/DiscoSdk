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
            ValidateIntent(DiscordIntent.MessageContent, "access message content");
            return Message.Content;
        }
    }
    public MessageComponent[]? Components => Message.Components;
    public IReaction[] Reactions { get; }
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

    public ISendMessageRestAction Reply(string? content = null)
    {
        return new SendMessageRestAction(_client, null, Channel, content)
            .SetMessageReference(Message.Id.ToString(), Message.ChannelId.ToString(), Message.GuildId?.ToString());
    }

    // Direct Operations
    /// <inheritdoc />
    public IRestAction Delete()
    {
        if (Message.Flags.HasFlag(MessageFlags.Ephemeral) && _interactionHandle == null)
            throw EphemeralMessageException.Operation("delete");

        return RestAction.Create(cancellationToken =>
        {
            if (_interactionHandle != null)
                return new WebhookMessageClient(_client.HttpClient).DeleteOriginalResponseAsync(_interactionHandle.WithAppId(_client.ApplicationId), cancellationToken);

            return _client.MessageClient.DeleteAsync(Message.ChannelId, Message.Id, cancellationToken);
        });
    }

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

    public IRestAction<User[]> GetReactions(string emoji, string? after = null, int? limit = null)
    {
        ValidateReactionIntent("get reactions from");

        if (Message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("get reactions from");

        return RestAction<User[]>.Create(cancellationToken =>
            _client.MessageClient.GetReactionsAsync(Message.ChannelId, Message.Id, emoji, after, limit, cancellationToken));
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
        return Message.GuildId.HasValue
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
        if (string.IsNullOrEmpty(_client.BotUser?.Id))
            return false;

        if (!Snowflake.TryParse(_client.BotUser.Id, out var botId))
            return false;

        return Message.Author.UserId == botId;
    }
}