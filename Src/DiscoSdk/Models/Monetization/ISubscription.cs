using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Monetization;

/// <summary>
/// Read-only view of a Discord subscription.
/// </summary>
public interface ISubscription
{
	/// <summary>The ID of the subscription.</summary>
	Snowflake Id { get; }

	/// <summary>The ID of the user who is subscribed.</summary>
	Snowflake UserId { get; }

	/// <summary>The SKUs subscribed to.</summary>
	IReadOnlyList<Snowflake> SkuIds { get; }

	/// <summary>The entitlements granted for this subscription.</summary>
	IReadOnlyList<Snowflake> EntitlementIds { get; }

	/// <summary>The SKUs that this subscription will renew into at the start of the next period.</summary>
	IReadOnlyList<Snowflake>? RenewalSkuIds { get; }

	/// <summary>Start (ISO 8601) of the current subscription period.</summary>
	string CurrentPeriodStart { get; }

	/// <summary>End (ISO 8601) of the current subscription period.</summary>
	string CurrentPeriodEnd { get; }

	/// <summary>The current status of the subscription.</summary>
	SubscriptionStatus Status { get; }

	/// <summary>When the subscription was canceled (ISO 8601); only present with the <c>ENDING</c> status.</summary>
	string? CanceledAt { get; }

	/// <summary>ISO 3166-1 alpha-2 country code of the payment source.</summary>
	string? Country { get; }
}
