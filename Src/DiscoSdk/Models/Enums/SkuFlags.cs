namespace DiscoSdk.Models.Enums;

/// <summary>
/// Bit flags describing where and how a Discord SKU can be purchased.
/// </summary>
[Flags]
public enum SkuFlags
{
	/// <summary>No flags set.</summary>
	None = 0,

	/// <summary>The SKU is available for purchase.</summary>
	Available = 1 << 2,

	/// <summary>A subscription purchased by a user and applied to a single guild — everyone in that guild gets access.</summary>
	GuildSubscription = 1 << 7,

	/// <summary>A subscription purchased by a user for themselves — they get access in every guild.</summary>
	UserSubscription = 1 << 8
}
