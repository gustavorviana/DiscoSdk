using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="ITextBasedChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TextBasedChannelWrapper"/> class.
/// </remarks>
/// <param name="channel">The channel instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class TextBasedChannelWrapper(Channel channel, DiscordClient client)
    : ChannelWrapper(channel, client), ITextBasedChannel
{
    public DiscordId? LastMessageId => _channel.LastMessageId;
    public int? RateLimitPerUser => _channel.RateLimitPerUser;
    public string? Topic => _channel.Topic;
    public DateTimeOffset? LastPinTimestamp => _channel.LastPinTimestamp != null
        ? DateTimeOffset.Parse(_channel.LastPinTimestamp)
        : null;

    public async Task<IMessage[]> GetMessagesAsync(int? limit = null, DiscordId? around = null, DiscordId? before = null, DiscordId? after = null, CancellationToken cancellationToken = default)
    {
        var messages = await _client.ChannelClient.GetMessagesAsync(_channel.Id, limit, around, before, after, cancellationToken);
        return [.. messages.Select(m => new MessageWrapper(this, m, _client, null)).Cast<IMessage>()];
    }

    public async Task<IMessage?> GetMessageAsync(DiscordId messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await _client.ChannelClient.GetMessageAsync(_channel.Id, messageId, cancellationToken);
            return new MessageWrapper(this, message, _client, null);
        }
        catch
        {
            return null;
        }
    }

    public Task DeleteMessageAsync(DiscordId messageId, CancellationToken cancellationToken = default)
    {
        return _client.ChannelClient.DeleteMessageAsync(_channel.Id, messageId, cancellationToken);
    }

    public Task BulkDeleteMessagesAsync(DiscordId[] messageIds, CancellationToken cancellationToken = default)
    {
        return _client.ChannelClient.BulkDeleteMessagesAsync(_channel.Id, messageIds, cancellationToken);
    }

    public Task TriggerTypingAsync(CancellationToken cancellationToken = default)
    {
        return _client.ChannelClient.TriggerTypingAsync(_channel.Id, cancellationToken);
    }

    public Task PurgeMessagesByIdAsync(params DiscordId[] messageIds)
    {
        return Task.WhenAll(messageIds.Select(id => DeleteMessageAsync(id)));
    }

    public Task PurgeMessagesAsync(params IMessage[] messages)
    {
        return Task.WhenAll(messages.Select(m => DeleteMessageAsync(m.Id)));
    }

    public Task PurgeMessagesAsync(IEnumerable<IMessage> messages)
    {
        return Task.WhenAll(messages.Select(m => DeleteMessageAsync(m.Id)));
    }

    public ISendMessageRestAction SendMessage()
    {
        return new SendMessageRestAction(_client, null, this, null);
    }

    public IMessagePaginationAction GetMessages()
    {
        return new MessagePaginationAction(_client, this);
    }

    public Task AddReactionByIdAsync(DiscordId messageId, Emoji emoji, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.AddReactionAsync(_channel.Id, messageId, emoji.ToString(), cancellationToken);
    }

    public Task RemoveReactionByIdAsync(DiscordId messageId, Emoji emoji, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.RemoveReactionAsync(_channel.Id, messageId, emoji.ToString(), cancellationToken);
    }

    public IReactionPaginationAction RetrieveReactionUsersById(DiscordId messageId, Emoji emoji)
    {
        return new ReactionPaginationAction(_client, _channel.Id, messageId, emoji);
    }

    public IReactionPaginationAction RetrieveReactionUsersById(DiscordId messageId, Emoji emoji, ReactionType type)
    {
        return new ReactionPaginationAction(_client, _channel.Id, messageId, emoji);
    }

    public Task PinMessageByIdAsync(DiscordId messageId, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.PinAsync(_channel.Id, messageId, cancellationToken);
    }

    public Task UnpinMessageByIdAsync(DiscordId messageId, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.UnpinAsync(_channel.Id, messageId, cancellationToken);
    }

    public IRestAction<IMessage[]> RetrievePinnedMessages()
    {
        return RestAction<IMessage[]>.Create(async cancellationToken =>
        {
            var messages = await _client.MessageClient.GetPinnedMessagesAsync(_channel.Id, cancellationToken);
            return [.. messages.Select(m => new MessageWrapper(this, m, _client, null)).Cast<IMessage>()];
        });
    }

    public IEditMessageRestAction EditMessageById(DiscordId messageId)
    {
        return new EditMessageRestAction(_client, this, messageId, null);
    }

    public async Task<IMessage> EndPollByIdAsync(DiscordId messageId, CancellationToken cancellationToken = default)
    {
        var message = await _client.MessageClient.EndPollAsync(_channel.Id, messageId, cancellationToken);
        return new MessageWrapper(this, message, _client, null);
    }

    public IPollVotersPaginationAction RetrievePollVotersById(DiscordId messageId, ulong answerId)
    {
        return new PollVotersPaginationAction(_client, _channel.Id, messageId, answerId);
    }
}