using DiscoSdk.Models;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for guild template (<c>/guilds/templates</c>, <c>/guilds/{guild_id}/templates</c>) and
/// guild onboarding (<c>/guilds/{guild_id}/onboarding</c>) operations.
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class GuildTemplateClient(IDiscordRestClient client)
{
	// ---- Templates ----

	/// <summary>Gets a guild template by its code.</summary>
	public Task<GuildTemplate> GetTemplateAsync(string code, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);

		var route = new DiscordRoute("guilds/templates/{code}", code);
		return client.SendAsync<GuildTemplate>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Creates a new guild from a template.</summary>
	/// <param name="icon">Optional base64 128x128 image data URI for the guild icon.</param>
	public Task<Guild> CreateGuildFromTemplateAsync(string code, string name, string? icon = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		var route = new DiscordRoute("guilds/templates/{code}", code);
		return client.SendAsync<Guild>(route, HttpMethod.Post, new { name, icon }, cancellationToken);
	}

	/// <summary>Lists the templates owned by a guild.</summary>
	public Task<GuildTemplate[]> GetGuildTemplatesAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		var route = new DiscordRoute("guilds/{guild_id}/templates", guildId);
		return client.SendAsync<GuildTemplate[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Creates a template for a guild.</summary>
	public Task<GuildTemplate> CreateGuildTemplateAsync(Snowflake guildId, string name, string? description = null, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		var route = new DiscordRoute("guilds/{guild_id}/templates", guildId);
		return client.SendAsync<GuildTemplate>(route, HttpMethod.Post, new { name, description }, cancellationToken);
	}

	/// <summary>Syncs a guild template to the guild's current state.</summary>
	public Task<GuildTemplate> SyncGuildTemplateAsync(Snowflake guildId, string code, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		ArgumentException.ThrowIfNullOrWhiteSpace(code);

		var route = new DiscordRoute("guilds/{guild_id}/templates/{code}", guildId, code);
		return client.SendAsync<GuildTemplate>(route, HttpMethod.Put, null, cancellationToken);
	}

	/// <summary>Modifies a guild template's name and/or description.</summary>
	public Task<GuildTemplate> ModifyGuildTemplateAsync(Snowflake guildId, string code, string? name = null, string? description = null, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		ArgumentException.ThrowIfNullOrWhiteSpace(code);

		var route = new DiscordRoute("guilds/{guild_id}/templates/{code}", guildId, code);
		return client.SendAsync<GuildTemplate>(route, HttpMethod.Patch, new { name, description }, cancellationToken);
	}

	/// <summary>Deletes a guild template.</summary>
	public Task<GuildTemplate> DeleteGuildTemplateAsync(Snowflake guildId, string code, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		ArgumentException.ThrowIfNullOrWhiteSpace(code);

		var route = new DiscordRoute("guilds/{guild_id}/templates/{code}", guildId, code);
		return client.SendAsync<GuildTemplate>(route, HttpMethod.Delete, null, cancellationToken);
	}

	// ---- Onboarding ----

	/// <summary>Gets the onboarding configuration for a guild.</summary>
	public Task<GuildOnboarding> GetOnboardingAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		var route = new DiscordRoute("guilds/{guild_id}/onboarding", guildId);
		return client.SendAsync<GuildOnboarding>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Modifies the onboarding configuration for a guild.</summary>
	public Task<GuildOnboarding> ModifyOnboardingAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/onboarding", guildId);
		return client.SendAsync<GuildOnboarding>(route, HttpMethod.Put, request, cancellationToken);
	}
}
