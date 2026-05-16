namespace DiscoSdk.Models;

/// <summary>
/// Read-only tags that classify a role (bot-owned, integration-owned, premium-subscriber, etc.).
/// </summary>
public interface IRoleTags
{
	/// <summary>Bot id this role was created for, when the role is bot-owned.</summary>
	Snowflake? BotId { get; }

	/// <summary>Integration id this role was created by, when the role is integration-owned.</summary>
	Snowflake? IntegrationId { get; }

	/// <summary>Present (any non-null payload) when this is the guild's Booster/premium-subscriber role.</summary>
	object? PremiumSubscriber { get; }

	/// <summary>Linked subscription listing id, when this role gates a paid subscription.</summary>
	Snowflake? SubscriptionListingId { get; }

	/// <summary>Present when the role represents an item available for purchase.</summary>
	object? AvailableForPurchase { get; }

	/// <summary>Present when the role is awarded to users linked to a configured guild connection.</summary>
	object? GuildConnections { get; }
}
