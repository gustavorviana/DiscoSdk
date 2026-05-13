using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Monetization;

/// <summary>
/// A Discord SKU (a premium offering), with the operations that can be performed on it.
/// </summary>
public interface ISku
{
	/// <summary>The ID of the SKU.</summary>
	Snowflake Id { get; }

	/// <summary>The type of SKU.</summary>
	SkuType Type { get; }

	/// <summary>The ID of the parent application.</summary>
	Snowflake ApplicationId { get; }

	/// <summary>The customer-facing name of the premium offering.</summary>
	string Name { get; }

	/// <summary>The system-generated URL slug based on the SKU's name.</summary>
	string Slug { get; }

	/// <summary>Flags describing how the SKU can be purchased.</summary>
	SkuFlags Flags { get; }

	/// <summary>Creates a REST action that lists the subscriptions for this SKU.</summary>
	/// <param name="userId">If set, only the subscription owned by this user.</param>
	IRestAction<IReadOnlyList<ISubscription>> GetSubscriptions(Snowflake? userId = null);

	/// <summary>Creates a REST action that retrieves a single subscription for this SKU.</summary>
	IRestAction<ISubscription> GetSubscription(Snowflake subscriptionId);
}
