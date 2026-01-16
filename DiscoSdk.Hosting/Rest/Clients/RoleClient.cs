using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord role operations (create, edit, delete, modify position, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class RoleClient(IDiscordRestClientBase client)
{
	/// <summary>
	/// Creates a new role in the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild to create the role in.</param>
	/// <param name="request">The role creation request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The created role.</returns>
	public Task<Role> CreateAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"guilds/{guildId}/roles";
		return client.SendAsync<Role>(path, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Edits an existing role in the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild containing the role.</param>
	/// <param name="roleId">The ID of the role to edit.</param>
	/// <param name="request">The role edit request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The edited role.</returns>
	public Task<Role> EditAsync(Snowflake guildId, Snowflake roleId, object request, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (roleId == default)
			throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"guilds/{guildId}/roles/{roleId}";
		return client.SendAsync<Role>(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Deletes a role from the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild containing the role.</param>
	/// <param name="roleId">The ID of the role to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task DeleteAsync(Snowflake guildId, Snowflake roleId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		if (roleId == default)
			throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));

		var path = $"guilds/{guildId}/roles/{roleId}";
		return client.SendAsync<object>(path, HttpMethod.Delete, null, cancellationToken);
	}

	/// <summary>
	/// Modifies the positions of roles in the specified guild.
	/// </summary>
	/// <param name="guildId">The ID of the guild containing the roles.</param>
	/// <param name="request">The role position modification request (array of {id, position}).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of roles with updated positions.</returns>
	public Task<Role[]> ModifyPositionsAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"guilds/{guildId}/roles";
		return client.SendAsync<Role[]>(path, HttpMethod.Patch, request, cancellationToken);
	}
}

