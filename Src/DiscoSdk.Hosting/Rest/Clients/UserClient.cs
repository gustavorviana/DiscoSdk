using DiscoSdk.Exceptions;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using System.Text;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord user operations (get user, current-user endpoints, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class UserClient(IDiscordRestClient client)
{
	/// <summary>
	/// Gets a user by their ID.
	/// </summary>
	/// <param name="userId">The ID of the user to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The user, or null if not found.</returns>
	public async Task<User?> GetAsync(Snowflake userId, CancellationToken cancellationToken = default)
	{
		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var route = new DiscordRoute("users/{user_id}", userId);
		try
		{
			return await client.SendAsync<User>(route, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	/// <summary>Gets the current user (the bot).</summary>
	public Task<User> GetCurrentAsync(CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("users/@me");
		return client.SendAsync<User>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Modifies the current user. <paramref name="request"/> is a partial body with any subset of
	/// <c>username</c>, <c>avatar</c> (base64 data URI) and <c>banner</c> (base64 data URI).
	/// </summary>
	public Task<User> ModifyCurrentAsync(object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		var route = new DiscordRoute("users/@me");
		return client.SendAsync<User>(route, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Lists the guilds the current user (bot) is in. Paginated by <c>before</c>/<c>after</c> guild ID.
	/// </summary>
	public Task<Guild[]> GetCurrentGuildsAsync(int? limit = null, Snowflake? before = null, Snowflake? after = null, bool? withCounts = null, CancellationToken cancellationToken = default)
	{
		if (limit.HasValue && (limit.Value < 1 || limit.Value > 200))
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 200.");

		var sb = new StringBuilder("users/@me/guilds");
		var hasQuery = false;
		void Append(string k, string v) { sb.Append(hasQuery ? '&' : '?').Append(k).Append('=').Append(v); hasQuery = true; }
		if (limit.HasValue) Append("limit", limit.Value.ToString());
		if (before.HasValue) Append("before", before.Value.ToString());
		if (after.HasValue) Append("after", after.Value.ToString());
		if (withCounts.HasValue) Append("with_counts", withCounts.Value ? "true" : "false");

		var route = new DiscordRoute(sb.ToString());
		return client.SendAsync<Guild[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets the current user's guild member object for a guild.</summary>
	public Task<GuildMember> GetCurrentGuildMemberAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("users/@me/guilds/{guild_id}/member", guildId);
		return client.SendAsync<GuildMember>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Lists the current user's third-party connections (Steam, Twitch, etc.).</summary>
	public Task<Connection[]> GetConnectionsAsync(CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("users/@me/connections");
		return client.SendAsync<Connection[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets the current user's role-connection data for an application (linked-roles).</summary>
	public Task<ApplicationRoleConnection> GetApplicationRoleConnectionAsync(Snowflake applicationId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("users/@me/applications/{application_id}/role-connection", applicationId);
		return client.SendAsync<ApplicationRoleConnection>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Updates the current user's role-connection data for an application. <paramref name="request"/>
	/// is the <see cref="ApplicationRoleConnection"/>-shaped body to PUT.
	/// </summary>
	public Task<ApplicationRoleConnection> UpdateApplicationRoleConnectionAsync(Snowflake applicationId, object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		var route = new DiscordRoute("users/@me/applications/{application_id}/role-connection", applicationId);
		return client.SendAsync<ApplicationRoleConnection>(route, HttpMethod.Put, request, cancellationToken);
	}
}

