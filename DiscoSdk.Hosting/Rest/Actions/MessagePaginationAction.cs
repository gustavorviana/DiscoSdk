using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IMessagePaginationAction"/> for retrieving messages from a channel.
/// </summary>
internal class MessagePaginationAction : RestAction<IMessage[]>, IMessagePaginationAction
{
	private readonly DiscordClient _client;
	private readonly ITextBasedChannel _channel;
	private int? _limit;
	private Snowflake? _around;
	private Snowflake? _before;
	private Snowflake? _after;

	/// <summary>
	/// Initializes a new instance of the <see cref="MessagePaginationAction"/> class.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="channel">The ID of the channel to get messages from.</param>
	public MessagePaginationAction(DiscordClient client, ITextBasedChannel channel)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_channel = channel;
	}

	/// <inheritdoc />
	public IMessagePaginationAction Limit(int limit)
	{
		if (limit < 1 || limit > 100)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		_limit = limit;
		return this;
	}

	/// <inheritdoc />
	public IMessagePaginationAction Around(Snowflake messageId)
	{
		_around = messageId;
		_before = null;
		_after = null;
		return this;
	}

	/// <inheritdoc />
	public IMessagePaginationAction After(Snowflake messageId)
	{
		_after = messageId;
		_around = null;
		_before = null;
		return this;
	}

	/// <inheritdoc />
	public IMessagePaginationAction Before(Snowflake messageId)
	{
		_before = messageId;
		_around = null;
		_after = null;
		return this;
	}

	/// <inheritdoc />
	public IMessagePaginationAction FromBeginning()
	{
		_around = null;
		_before = null;
		_after = null;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IMessage[]> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var channel = await _client.ChannelClient.GetAsync(_channel.Id, cancellationToken);
		var messages = await _client.ChannelClient.GetMessagesAsync(_channel.Id, _limit, _around, _before, _after, cancellationToken);
		return [.. messages.Select(m => new MessageWrapper(_channel, m, _client, null)).Cast<IMessage>()];
	}
}