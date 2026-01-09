using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IMemberPaginationAction"/> for retrieving guild members.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MemberPaginationAction"/> class.
/// </remarks>
/// <param name="client">The Discord client.</param>
/// <param name="guildId">The ID of the guild to get members from.</param>
internal class MemberPaginationAction(DiscordClient client, IGuild guild) : RestAction<IMember[]>, IMemberPaginationAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private int? _limit;
	private DiscordId? _after;

    /// <inheritdoc />
    public IMemberPaginationAction Limit(int limit)
	{
		if (limit < 1 || limit > 1000)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 1000.");

		_limit = limit;
		return this;
	}

	/// <inheritdoc />
	public IMemberPaginationAction After(DiscordId userId)
	{
		_after = userId;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IMember[]> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var members = await _client.GuildClient.GetMembersAsync(guild.Id, _limit, _after, cancellationToken);
		
		return [.. members
			.Where(m => m.User != null)
			.Select(m => new GuildMemberWrapper(m, guild, _client))
			.Cast<IMember>()];
	}
}