using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IReactionPaginationAction"/> for retrieving reaction users.
/// </summary>
internal class ReactionPaginationAction : RestAction<User[]>, IReactionPaginationAction
{
	private readonly DiscordClient _client;
	private readonly Snowflake _channelId;
	private readonly Snowflake _messageId;
	private readonly string _emoji;
	private Snowflake? _after;
	private int? _limit;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReactionPaginationAction"/> class.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to get reactions for.</param>
	/// <param name="reactionType">Optional reaction type filter.</param>
	public ReactionPaginationAction(DiscordClient client, Snowflake channelId, Snowflake messageId, Emoji emoji)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_channelId = channelId;
		_messageId = messageId;
		_emoji = emoji.ToString();
	}

    public IReactionPaginationAction After(Snowflake userId)
    {
		_after = userId;
        return this;
    }

    /// <inheritdoc />
    public IReactionPaginationAction Limit(int limit)
	{
		if (limit < 1 || limit > 100)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		_limit = limit;
		return this;
	}

	/// <inheritdoc />
	public override async Task<User[]> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		return await _client.MessageClient.GetReactionsAsync(_channelId, _messageId, _emoji, _after?.ToString(), _limit, cancellationToken);
	}
}