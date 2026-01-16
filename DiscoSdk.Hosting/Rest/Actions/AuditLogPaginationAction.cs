using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IAuditLogPaginationAction"/> for retrieving audit logs from a guild.
/// </summary>
internal class AuditLogPaginationAction : RestAction<AuditLogEntry[]>, IAuditLogPaginationAction
{
	private readonly DiscordClient _client;
	private readonly IGuild _guild;
	private int? _limit;
	private Snowflake? _before;
	private Snowflake? _userId;
	private AuditLogActionType? _actionType;

	/// <summary>
	/// Initializes a new instance of the <see cref="AuditLogPaginationAction"/> class.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="guild">The guild to get audit logs from.</param>
	public AuditLogPaginationAction(DiscordClient client, IGuild guild)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guild = guild ?? throw new ArgumentNullException(nameof(guild));
	}

	/// <inheritdoc />
	public IAuditLogPaginationAction Limit(int limit)
	{
		if (limit < 1 || limit > 100)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		_limit = limit;
		return this;
	}

	/// <inheritdoc />
	public IAuditLogPaginationAction Before(Snowflake entryId)
	{
		_before = entryId;
		return this;
	}

	/// <inheritdoc />
	public IAuditLogPaginationAction SetUserId(Snowflake? userId)
	{
		_userId = userId;
		return this;
	}

	/// <inheritdoc />
	public IAuditLogPaginationAction SetActionType(AuditLogActionType? actionType)
	{
		_actionType = actionType;
		return this;
	}

	/// <inheritdoc />
	public override async Task<AuditLogEntry[]> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var entries = await _client.GuildClient.GetAuditLogsAsync(
			_guild.Id,
			_limit,
			_before,
			_userId,
			_actionType,
			cancellationToken);

		return entries;
	}
}