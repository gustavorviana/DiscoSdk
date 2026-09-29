using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IPinnedMessagePaginationAction"/> over
/// <c>GET /channels/{channel.id}/messages/pins</c>.
/// </summary>
internal class PinnedMessagePaginationAction : RestAction<IPinnedMessage[]>, IPinnedMessagePaginationAction
{
	private readonly DiscordClient _client;
	private readonly ITextBasedChannel _channel;
	private int? _limit;
	private DateTimeOffset? _before;

	/// <summary>
	/// Initializes a new instance of the <see cref="PinnedMessagePaginationAction"/> class.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="channel">The channel to get pinned messages from.</param>
	public PinnedMessagePaginationAction(DiscordClient client, ITextBasedChannel channel)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_channel = channel ?? throw new ArgumentNullException(nameof(channel));
	}

	/// <inheritdoc />
	public IPinnedMessagePaginationAction Limit(int limit)
	{
		if (limit < 1 || limit > 50)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 50.");

		_limit = limit;
		return this;
	}

	/// <inheritdoc />
	public IPinnedMessagePaginationAction Before(DateTimeOffset pinnedBefore)
	{
		_before = pinnedBefore;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IPinnedMessage[]> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var response = await _client.MessageClient.GetPinnedMessagesAsync(_channel.Id, _before, _limit, cancellationToken);
		return [.. response.Items.Select(pin => new PinnedMessage(pin.PinnedAt, new MessageWrapper(_client, _channel, pin.Message, null)))];
	}

	private sealed record PinnedMessage(DateTimeOffset PinnedAt, IMessage Message) : IPinnedMessage;
}
