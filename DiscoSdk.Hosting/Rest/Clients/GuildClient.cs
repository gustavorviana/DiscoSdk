using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord guild operations (get members, channels, roles, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class GuildClient(IDiscordRestClientBase client)
{
	/// <summary>
	/// Gets a list of members in the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="limit">Maximum number of members to return (1-1000, default 1).</param>
	/// <param name="after">Get members after this user ID.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of guild members.</returns>
	public Task<GuildMember[]> GetMembersAsync(DiscordId guildId, int? limit = null, DiscordId? after = null, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (limit.HasValue && (limit.Value < 1 || limit.Value > 1000))
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 1000.");

		var queryParams = new List<string>();

		if (limit.HasValue)
			queryParams.Add($"limit={limit.Value}");

		if (after.HasValue)
			queryParams.Add($"after={after.Value}");

		var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
		var path = $"guilds/{guildId}/members{query}";
		return client.SendJsonAsync<GuildMember[]>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Gets a list of channels in the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of channels.</returns>
	public Task<Channel[]> GetChannelsAsync(DiscordId guildId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		var path = $"guilds/{guildId}/channels";
		return client.SendJsonAsync<Channel[]>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Gets a list of roles in the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of roles.</returns>
	public Task<Role[]> GetRolesAsync(DiscordId guildId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		var path = $"guilds/{guildId}/roles";
		return client.SendJsonAsync<Role[]>(path, HttpMethod.Get, null, cancellationToken);
	}
}

