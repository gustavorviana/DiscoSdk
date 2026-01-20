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
internal class TextBasedChannelWrapper(DiscordClient client, Channel channel)
    : ChannelWrapper(client, channel), ITextBasedChannel
{
    public Snowflake? LastMessageId => _channel.LastMessageId;
    public int? RateLimitPerUser => _channel.RateLimitPerUser;
    public string? Topic => _channel.Topic;
    public DateTimeOffset? LastPinTimestamp => _channel.LastPinTimestamp != null
        ? DateTimeOffset.Parse(_channel.LastPinTimestamp)
        : null;

    public IRestAction<IMessage[]> GetMessagesAsync(int? limit = null, Snowflake? around = null, Snowflake? before = null, Snowflake? after = null, CancellationToken cancellationToken = default)
    {
        return RestAction<IMessage[]>.Create(async cancellationToken =>
        {
            var messages = await _client.ChannelClient.GetMessagesAsync(_channel.Id, limit, around, before, after, cancellationToken);
            return [.. messages.Select(m => new MessageWrapper(_client, this, m, null)).Cast<IMessage>()];
        });
    }

    public IRestAction<IMessage?> GetMessageAsync(Snowflake messageId, CancellationToken cancellationToken = default)
    {
        return RestAction<IMessage?>.Create(async cancellationToken =>
        {
            try
            {
                var message = await _client.ChannelClient.GetMessageAsync(_channel.Id, messageId, cancellationToken);
                return new MessageWrapper(_client, this, message, null);
            }
            catch
            {
                return null;
            }
        });
    }

    public IRestAction DeleteMessageAsync(Snowflake messageId, CancellationToken cancellationToken = default)
    {
        return RestAction.Create(cancellationToken =>
        {
            return _client.ChannelClient.DeleteMessageAsync(_channel.Id, messageId, cancellationToken);
        });
    }

    public IRestAction BulkDeleteMessagesAsync(Snowflake[] messageIds, CancellationToken cancellationToken = default)
    {
        return RestAction.Create(cancellationToken =>
        {
            return _client.ChannelClient.BulkDeleteMessagesAsync(_channel.Id, messageIds, cancellationToken);
        });
    }

    public IRestAction TriggerTypingAsync(CancellationToken cancellationToken = default)
    {
        return RestAction.Create(cancellationToken =>
        {
            return _client.ChannelClient.TriggerTypingAsync(_channel.Id, cancellationToken);
        });
    }

    public IRestAction PurgeMessagesByIdAsync(params Snowflake[] messageIds)
    {
        return RestAction.Create(cancellationToken =>
        {
            return Task.WhenAll(messageIds.Select(id => DeleteMessageAsync(id).ExecuteAsync(cancellationToken)));
        });
    }

    public IRestAction PurgeMessagesAsync(params IMessage[] messages)
    {
        return RestAction.Create(async cancellationToken =>
        {
            await Task.WhenAll(messages.Select(m => DeleteMessageAsync(m.Id).ExecuteAsync(cancellationToken)));
        });
    }

    public IRestAction PurgeMessagesAsync(IEnumerable<IMessage> messages)
    {
        return RestAction.Create(async cancellationToken =>
        {
            await Task.WhenAll(messages.Select(m => DeleteMessageAsync(m.Id).ExecuteAsync(cancellationToken)));
        });
    }

    public ISendMessageRestAction SendMessage(string? content = null)
    {
        return new SendMessageRestAction(_client, null, this, content);
    }

    public IMessagePaginationAction GetMessages()
    {
        return new MessagePaginationAction(_client, this);
    }

    public IRestAction AddReactionByIdAsync(Snowflake messageId, Emoji emoji, CancellationToken cancellationToken = default)
    {
        return RestAction.Create(cancellationToken =>
        {
            return _client.MessageClient.AddReactionAsync(_channel.Id, messageId, emoji.ToString(), cancellationToken);
        });
    }

    public IRestAction RemoveReactionByIdAsync(Snowflake messageId, Emoji emoji, CancellationToken cancellationToken = default)
    {
        return RestAction.Create(cancellationToken =>
        {
            return _client.MessageClient.RemoveReactionAsync(_channel.Id, messageId, emoji.ToString(), cancellationToken);
        });
    }

    public IReactionPaginationAction RetrieveReactionUsersById(Snowflake messageId, Emoji emoji)
    {
        return new ReactionPaginationAction(_client, _channel.Id, messageId, emoji);
    }

    public IReactionPaginationAction RetrieveReactionUsersById(Snowflake messageId, Emoji emoji, ReactionType type)
    {
        return new ReactionPaginationAction(_client, _channel.Id, messageId, emoji);
    }

    public IRestAction PinMessageByIdAsync(Snowflake messageId, CancellationToken cancellationToken = default)
    {
        return RestAction.Create(cancellationToken =>
        {
            return _client.MessageClient.PinAsync(_channel.Id, messageId, cancellationToken);
        });
    }

    public IRestAction UnpinMessageByIdAsync(Snowflake messageId, CancellationToken cancellationToken = default)
    {
        return RestAction.Create(cancellationToken =>
        {
            return _client.MessageClient.UnpinAsync(_channel.Id, messageId, cancellationToken);
        });
    }

    public IRestAction<IMessage[]> RetrievePinnedMessages()
    {
        return RestAction<IMessage[]>.Create(async cancellationToken =>
        {
            var messages = await _client.MessageClient.GetPinnedMessagesAsync(_channel.Id, cancellationToken);
            return [.. messages.Select(m => new MessageWrapper(_client, this, m, null)).Cast<IMessage>()];
        });
    }

    public IEditMessageRestAction EditMessageById(Snowflake messageId)
    {
        return new EditMessageRestAction(_client, this, messageId, null);
    }

    public IRestAction<IMessage> EndPollByIdAsync(Snowflake messageId, CancellationToken cancellationToken = default)
    {
        return RestAction<IMessage>.Create(async cancellationToken =>
        {
            var message = await _client.MessageClient.EndPollAsync(_channel.Id, messageId, cancellationToken);
            return new MessageWrapper(_client, this, message, null);
        });
    }

    public IPollVotersPaginationAction RetrievePollVotersById(Snowflake messageId, ulong answerId)
    {
        return new PollVotersPaginationAction(_client, _channel.Id, messageId, answerId);
    }
}