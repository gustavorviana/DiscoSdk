namespace DiscoSdk.Models.Enums;

/// <summary>
/// The type of a Discord entitlement (a user's or guild's access to an SKU).
/// </summary>
public enum EntitlementType
{
	/// <summary>Entitlement was purchased by a user.</summary>
	Purchase = 1,

	/// <summary>Entitlement for a Discord Nitro subscription.</summary>
	PremiumSubscription = 2,

	/// <summary>Entitlement was gifted by a developer.</summary>
	DeveloperGift = 3,

	/// <summary>Entitlement was purchased by a developer in application test mode.</summary>
	TestModePurchase = 4,

	/// <summary>Entitlement was granted when the SKU was free.</summary>
	FreePurchase = 5,

	/// <summary>Entitlement was gifted by another user.</summary>
	UserGift = 6,

	/// <summary>Entitlement was claimed by a user for free as a Nitro subscriber.</summary>
	PremiumPurchase = 7,

	/// <summary>Entitlement was purchased as an app subscription.</summary>
	ApplicationSubscription = 8
}
