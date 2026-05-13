using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord auto-moderation operations (<c>/guilds/{guild_id}/auto-moderation/rules</c>).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class AutoModerationClient(IDiscordRestClient client)
{
	/// <summary>Lists all auto-moderation rules for a guild.</summary>
	public Task<AutoModerationRule[]> ListRulesAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

		var route = new DiscordRoute("guilds/{guild_id}/auto-moderation/rules", guildId);
		return client.SendAsync<AutoModerationRule[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets a single auto-moderation rule for a guild.</summary>
	public Task<AutoModerationRule> GetRuleAsync(Snowflake guildId, Snowflake ruleId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		if (ruleId == default)
			throw new ArgumentException("Rule ID cannot be null or empty.", nameof(ruleId));

		var route = new DiscordRoute("guilds/{guild_id}/auto-moderation/rules/{rule_id}", guildId, ruleId);
		return client.SendAsync<AutoModerationRule>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Creates a new auto-moderation rule in a guild.</summary>
	public Task<AutoModerationRule> CreateRuleAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/auto-moderation/rules", guildId);
		return client.SendAsync<AutoModerationRule>(route, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>Modifies an existing auto-moderation rule.</summary>
	public Task<AutoModerationRule> ModifyRuleAsync(Snowflake guildId, Snowflake ruleId, object request, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		if (ruleId == default)
			throw new ArgumentException("Rule ID cannot be null or empty.", nameof(ruleId));
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/auto-moderation/rules/{rule_id}", guildId, ruleId);
		return client.SendAsync<AutoModerationRule>(route, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>Deletes an auto-moderation rule.</summary>
	public Task DeleteRuleAsync(Snowflake guildId, Snowflake ruleId, CancellationToken cancellationToken = default)
	{
		if (guildId == default)
			throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));
		if (ruleId == default)
			throw new ArgumentException("Rule ID cannot be null or empty.", nameof(ruleId));

		var route = new DiscordRoute("guilds/{guild_id}/auto-moderation/rules/{rule_id}", guildId, ruleId);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}
}
