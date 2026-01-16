using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;

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
	public Task<GuildMember[]> GetMembersAsync(Snowflake guildId, int? limit = null, Snowflake? after = null, CancellationToken cancellationToken = default)
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
	/// Gets a specific member in the specified guild by user ID.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="userId">The ID of the user.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The guild member, or null if the user is not a member of the guild.</returns>
	public async Task<GuildMember?> GetMemberAsync(Snowflake guildId, Snowflake userId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var path = $"guilds/{guildId}/members/{userId}";
		try
		{
			return await client.SendJsonAsync<GuildMember>(path, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	/// <summary>
	/// Gets a specific ban in the specified guild by user ID.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="userId">The ID of the banned user.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The ban information, or null if the user is not banned from the guild.</returns>
	public async Task<Ban?> GetBanAsync(Snowflake guildId, Snowflake userId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var path = $"guilds/{guildId}/bans/{userId}";
		try
		{
			return await client.SendJsonAsync<Ban>(path, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	/// <summary>
	/// Gets a list of channels in the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of channels.</returns>
	public Task<Channel[]> GetChannelsAsync(Snowflake guildId, CancellationToken cancellationToken = default)
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
	public Task<Role[]> GetRolesAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		var path = $"guilds/{guildId}/roles";
		return client.SendJsonAsync<Role[]>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Gets a guild by its ID.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The guild, or null if not found.</returns>
	public async Task<Guild?> GetAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		var path = $"guilds/{guildId}";
		try
		{
			return await client.SendJsonAsync<Guild>(path, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	/// <summary>
	/// Requests to speak in a stage channel.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="channelId">The ID of the stage channel.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task RequestToSpeakAsync(Snowflake guildId, Snowflake channelId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"guilds/{guildId}/voice-states/@me";
		var request = new { channel_id = channelId.ToString(), request_to_speak_timestamp = DateTimeOffset.UtcNow.ToString("o") };
		return client.SendJsonAsync<object>(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Cancels the request to speak in a stage channel.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="channelId">The ID of the stage channel.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task CancelRequestToSpeakAsync(Snowflake guildId, Snowflake channelId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"guilds/{guildId}/voice-states/@me";
		var request = new { channel_id = channelId.ToString(), request_to_speak_timestamp = (string?)null };
		return client.SendJsonAsync<object>(path, HttpMethod.Patch, request, cancellationToken);
	}
}

