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

namespace DiscoSdk.Hosting.Messages;

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
    public MessageWrapper(Message message, DiscordClient client, InteractionHandle? interactionHandle)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionHandle = interactionHandle;
    }

    // Visual Properties
    public DiscordId Id => _message.Id;
    public DiscordId ChannelId => _message.ChannelId;
    public DiscordId? GuildId => _message.GuildId;
    public User Author => _message.Author;
    public string Content => _message.Content;
    public Embed[] Embeds => _message.Embeds;
    public MessageComponent[]? Components => _message.Components;
    public Attachment[] Attachments => _message.Attachments;
    public Reaction[]? Reactions => _message.Reactions;
    public string Timestamp => _message.Timestamp;
    public string? EditedTimestamp => _message.EditedTimestamp;
    public bool Pinned => _message.Pinned;
    public MessageFlags? Flags => _message.Flags;
    public MessageType Type => _message.Type;

    // Operations with Builders
    public IEditMessageRestAction Edit()
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral) && _interactionHandle == null)
            throw EphemeralMessageException.Operation("edit");

        return new EditMessageRestAction(_client, _message.ChannelId, _message.Id, _interactionHandle);
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        return new SendMessageRestAction(_client, null, _message.ChannelId, content)
            .SetMessageReference(_message.Id.ToString(), _message.ChannelId.ToString(), _message.GuildId?.ToString());
    }

    // Direct Operations
    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral) && _interactionHandle == null)
            throw EphemeralMessageException.Operation("delete");

        if (_interactionHandle != null)
            return _client.InteractionClient.DeleteOriginalResponseAsync(_interactionHandle, cancellationToken);

        return _client.MessageClient.DeleteAsync(_message.ChannelId, _message.Id, cancellationToken);
    }

    public void Delete()
    {
        DeleteAsync().GetAwaiter().GetResult();
    }

    public async Task<IMessage> CrosspostAsync(CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("crosspost");

        var message = await _client.MessageClient.CrosspostAsync(_message.ChannelId, _message.Id, cancellationToken);
        return new MessageWrapper(message, _client, null);
    }

    public IMessage Crosspost()
    {
        return CrosspostAsync().GetAwaiter().GetResult();
    }

    // Reaction Operations
    public Task AddReactionAsync(string emoji, CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("add reactions to");

        return _client.MessageClient.AddReactionAsync(_message.ChannelId, _message.Id, emoji, cancellationToken);
    }

    public void AddReaction(string emoji)
    {
        AddReactionAsync(emoji).GetAwaiter().GetResult();
    }

    public Task RemoveReactionAsync(string emoji, CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("remove reactions from");

        return _client.MessageClient.RemoveReactionAsync(_message.ChannelId, _message.Id, emoji, cancellationToken);
    }

    public void RemoveReaction(string emoji)
    {
        RemoveReactionAsync(emoji).GetAwaiter().GetResult();
    }

    public Task RemoveUserReactionAsync(string emoji, string userId, CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("remove user reactions from");

        return _client.MessageClient.RemoveUserReactionAsync(_message.ChannelId, _message.Id, emoji, userId, cancellationToken);
    }

    public void RemoveUserReaction(string emoji, string userId)
    {
        RemoveUserReactionAsync(emoji, userId).GetAwaiter().GetResult();
    }

    public Task<User[]> GetReactionsAsync(string emoji, string? after = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("get reactions from");

        return _client.MessageClient.GetReactionsAsync(_message.ChannelId, _message.Id, emoji, after, limit, cancellationToken);
    }

    public User[] GetReactions(string emoji, string? after = null, int? limit = null)
    {
        return GetReactionsAsync(emoji, after, limit).GetAwaiter().GetResult();
    }

    public Task DeleteAllReactionsForEmojiAsync(string emoji, CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("delete reactions from");

        return _client.MessageClient.DeleteAllReactionsForEmojiAsync(_message.ChannelId, _message.Id, emoji, cancellationToken);
    }

    public void DeleteAllReactionsForEmoji(string emoji)
    {
        DeleteAllReactionsForEmojiAsync(emoji).GetAwaiter().GetResult();
    }

    public Task DeleteAllReactionsAsync(CancellationToken cancellationToken = default)
    {
        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("delete all reactions from");

        return _client.MessageClient.DeleteAllReactionsAsync(_message.ChannelId, _message.Id, cancellationToken);
    }

    public void DeleteAllReactions()
    {
        DeleteAllReactionsAsync().GetAwaiter().GetResult();
    }

    public Guild? GetGuild()
    {
        if (_message.GuildId == null)
            return null;

        return _client.Guilds.All.TryGetValue(_message.GuildId.Value, out var guild) ? guild : null;
    }
}