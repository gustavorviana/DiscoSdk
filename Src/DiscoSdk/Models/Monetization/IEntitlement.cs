using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Monetization;

/// <summary>
/// A Discord entitlement (a user's or guild's access to a premium SKU), with the operations that can
/// be performed on it.
/// </summary>
public interface IEntitlement
{
	/// <summary>The ID of the entitlement.</summary>
	Snowflake Id { get; }

	/// <summary>The ID of the SKU this entitlement grants access to.</summary>
	Snowflake SkuId { get; }

	/// <summary>The ID of the parent application.</summary>
	Snowflake ApplicationId { get; }

	/// <summary>The ID of the user that is granted access, if this is a user entitlement.</summary>
	Snowflake? UserId { get; }

	/// <summary>The type of entitlement.</summary>
	EntitlementType Type { get; }

	/// <summary>Whether the entitlement was deleted.</summary>
	bool Deleted { get; }

	/// <summary>Start date (ISO 8601) at which the entitlement is valid; <c>null</c> when it never expires.</summary>
	string? StartsAt { get; }

	/// <summary>End date (ISO 8601) at which the entitlement is no longer valid; <c>null</c> when it never expires.</summary>
	string? EndsAt { get; }

	/// <summary>The ID of the guild that is granted access, if this is a guild entitlement.</summary>
	Snowflake? GuildId { get; }

	/// <summary>For consumable items, whether the entitlement has been consumed.</summary>
	bool? Consumed { get; }

	/// <summary>Creates a REST action that marks this one-time-purchase consumable entitlement as consumed.</summary>
	IRestAction Consume();

	/// <summary>Creates a REST action that deletes this entitlement (only valid for test entitlements).</summary>
	IRestAction DeleteTest();
}
