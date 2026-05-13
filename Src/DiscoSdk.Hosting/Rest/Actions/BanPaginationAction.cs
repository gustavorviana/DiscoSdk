using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IBanPaginationAction"/> for retrieving banned users from a guild.
/// </summary>
internal class BanPaginationAction(DiscordClient client, Snowflake guildId) : RestAction<IBan[]>, IBanPaginationAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private int? _limit;
	private Snowflake? _before;
	private Snowflake? _after;

	public IBanPaginationAction Limit(int limit)
	{
		if (limit < 1 || limit > 1000)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 1000.");

		_limit = limit;
		return this;
	}

	public IBanPaginationAction Before(Snowflake userId)
	{
		_before = userId;
		return this;
	}

	public IBanPaginationAction After(Snowflake userId)
	{
		_after = userId;
		return this;
	}

	public override async Task<IBan[]> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var bans = await _client.GuildClient.GetBansAsync(guildId, _limit, _before, _after, cancellationToken);
		return [.. bans.Select(b => (IBan)new BanWrapper(b, _client))];
	}
}
