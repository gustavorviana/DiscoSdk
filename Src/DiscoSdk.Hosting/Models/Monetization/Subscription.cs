using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Monetization;

/// <summary>
/// Represents a Discord subscription — a user's recurring payment for at least one <see cref="Sku"/>,
/// which results in <see cref="Entitlement"/>s being granted for the duration of each period. Doubles
/// as the public read surface (<see cref="ISubscription"/>).
/// </summary>
internal class Subscription : ISubscription
{
	/// <summary>The ID of the subscription.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>The ID of the user who is subscribed.</summary>
	[JsonPropertyName("user_id")]
	public Snowflake UserId { get; set; } = default!;

	/// <summary>The SKUs subscribed to.</summary>
	[JsonPropertyName("sku_ids")]
	public Snowflake[] SkuIds { get; set; } = [];

	/// <summary>The entitlements granted for this subscription.</summary>
	[JsonPropertyName("entitlement_ids")]
	public Snowflake[] EntitlementIds { get; set; } = [];

	/// <summary>The SKUs that this subscription will renew into at the start of the next period.</summary>
	[JsonPropertyName("renewal_sku_ids")]
	public Snowflake[]? RenewalSkuIds { get; set; }

	/// <summary>Start (ISO 8601) of the current subscription period.</summary>
	[JsonPropertyName("current_period_start")]
	public string CurrentPeriodStart { get; set; } = default!;

	/// <summary>End (ISO 8601) of the current subscription period.</summary>
	[JsonPropertyName("current_period_end")]
	public string CurrentPeriodEnd { get; set; } = default!;

	/// <summary>The current status of the subscription.</summary>
	[JsonPropertyName("status")]
	public SubscriptionStatus Status { get; set; }

	/// <summary>When the subscription was canceled (ISO 8601); only present with the <c>ENDING</c> status.</summary>
	[JsonPropertyName("canceled_at")]
	public string? CanceledAt { get; set; }

	/// <summary>ISO 3166-1 alpha-2 country code of the payment source used to purchase the subscription (requires the OAuth2 scope).</summary>
	[JsonPropertyName("country")]
	public string? Country { get; set; }

	IReadOnlyList<Snowflake> ISubscription.SkuIds => SkuIds;
	IReadOnlyList<Snowflake> ISubscription.EntitlementIds => EntitlementIds;
	IReadOnlyList<Snowflake>? ISubscription.RenewalSkuIds => RenewalSkuIds;
}
