using DiscoSdk.Models;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord role operations (create, edit, delete, modify position, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class RoleClient(IDiscordRestClient client)
{
	/// <summary>
	/// Creates a new role in the specified guild.
	/// </summary>
	public Task<Role> CreateAsync(Snowflake guildId, object request, string? auditLogReason = null, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/roles", guildId);
		return (string.IsNullOrEmpty(auditLogReason) ? client.SendAsync<Role>(route, HttpMethod.Post, request, cancellationToken) : client.SendWithReasonAsync<Role>(route, HttpMethod.Post, request, auditLogReason, cancellationToken));
	}

	/// <summary>
	/// Edits an existing role in the specified guild.
	/// </summary>
	public Task<Role> EditAsync(Snowflake guildId, Snowflake roleId, object request, string? auditLogReason = null, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (roleId == default)
			throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));

		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/roles/{role_id}", guildId, roleId);
		return (string.IsNullOrEmpty(auditLogReason) ? client.SendAsync<Role>(route, HttpMethod.Patch, request, cancellationToken) : client.SendWithReasonAsync<Role>(route, HttpMethod.Patch, request, auditLogReason, cancellationToken));
	}

	/// <summary>
	/// Deletes a role from the specified guild.
	/// </summary>
	public Task DeleteAsync(Snowflake guildId, Snowflake roleId, string? auditLogReason = null, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (roleId == default)
			throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));

		var route = new DiscordRoute("guilds/{guild_id}/roles/{role_id}", guildId, roleId);
		return (string.IsNullOrEmpty(auditLogReason) ? client.SendAsync(route, HttpMethod.Delete, cancellationToken) : client.SendWithReasonAsync(route, HttpMethod.Delete, body: null, auditLogReason, cancellationToken));
	}

	/// <summary>
	/// Gets a single role by id. Discord exposed this endpoint in 2024 — until then callers had to
	/// list every role and filter client-side. Returns <c>null</c> if the role does not exist.
	/// </summary>
	public Task<Role?> GetAsync(Snowflake guildId, Snowflake roleId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (roleId == default)
			throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));

		var route = new DiscordRoute("guilds/{guild_id}/roles/{role_id}", guildId, roleId);
		return client.SendAsync<Role?>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Modifies the positions of roles in the specified guild.
	/// </summary>
	public Task<Role[]> ModifyPositionsAsync(Snowflake guildId, object request, string? auditLogReason = null, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/roles", guildId);
		return (string.IsNullOrEmpty(auditLogReason) ? client.SendAsync<Role[]>(route, HttpMethod.Patch, request, cancellationToken) : client.SendWithReasonAsync<Role[]>(route, HttpMethod.Patch, request, auditLogReason, cancellationToken));
	}
}
