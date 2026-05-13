namespace DiscoSdk.Models.Enums;

/// <summary>
/// The type of a Discord SKU (item available for purchase).
/// </summary>
public enum SkuType
{
	/// <summary>A durable one-time purchase.</summary>
	Durable = 2,

	/// <summary>A consumable one-time purchase.</summary>
	Consumable = 3,

	/// <summary>Represents a recurring subscription.</summary>
	Subscription = 5,

	/// <summary>System-generated group for each subscription SKU.</summary>
	SubscriptionGroup = 6
}
