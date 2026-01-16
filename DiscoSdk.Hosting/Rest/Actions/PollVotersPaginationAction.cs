using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IPollVotersPaginationAction"/> for retrieving poll voters.
/// </summary>
internal class PollVotersPaginationAction : RestAction<User[]>, IPollVotersPaginationAction
{
	private readonly DiscordClient _client;
	private readonly Snowflake _channelId;
	private readonly Snowflake _messageId;
	private readonly ulong _answerId;
	private int? _limit;
	private Snowflake? _after;

	/// <summary>
	/// Initializes a new instance of the <see cref="PollVotersPaginationAction"/> class.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="channelId">The ID of the channel containing the poll message.</param>
	/// <param name="messageId">The ID of the message containing the poll.</param>
	/// <param name="answerId">The ID of the poll answer.</param>
	public PollVotersPaginationAction(DiscordClient client, Snowflake channelId, Snowflake messageId, ulong answerId)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_channelId = channelId;
		_messageId = messageId;
		_answerId = answerId;
	}

    public IPollVotersPaginationAction After(Snowflake userId)
    {
		_after = userId;
        return this;
    }

    /// <inheritdoc />
    public IPollVotersPaginationAction Limit(int limit)
	{
		if (limit < 1 || limit > 100)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		_limit = limit;
		return this;
	}

	/// <inheritdoc />
	public override async Task<User[]> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		return await _client.MessageClient.GetPollVotersAsync(_channelId, _messageId, _answerId, _after?.ToString(), _limit, cancellationToken);
	}
}