using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Monetization;

/// <summary>
/// Represents a Discord entitlement — a user's or guild's access to a premium <see cref="Sku"/>.
/// </summary>
internal class Entitlement
{
	/// <summary>The ID of the entitlement.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>The ID of the SKU this entitlement grants access to.</summary>
	[JsonPropertyName("sku_id")]
	public Snowflake SkuId { get; set; } = default!;

	/// <summary>The ID of the parent application.</summary>
	[JsonPropertyName("application_id")]
	public Snowflake ApplicationId { get; set; } = default!;

	/// <summary>The ID of the user that is granted access, if this is a user entitlement.</summary>
	[JsonPropertyName("user_id")]
	public Snowflake? UserId { get; set; }

	/// <summary>The type of entitlement.</summary>
	[JsonPropertyName("type")]
	public EntitlementType Type { get; set; }

	/// <summary>Whether the entitlement was deleted.</summary>
	[JsonPropertyName("deleted")]
	public bool Deleted { get; set; }

	/// <summary>Start date (ISO 8601) at which the entitlement is valid; <c>null</c> when it never expires.</summary>
	[JsonPropertyName("starts_at")]
	public string? StartsAt { get; set; }

	/// <summary>End date (ISO 8601) at which the entitlement is no longer valid; <c>null</c> when it never expires.</summary>
	[JsonPropertyName("ends_at")]
	public string? EndsAt { get; set; }

	/// <summary>The ID of the guild that is granted access, if this is a guild entitlement.</summary>
	[JsonPropertyName("guild_id")]
	public Snowflake? GuildId { get; set; }

	/// <summary>For consumable items, whether the entitlement has been consumed.</summary>
	[JsonPropertyName("consumed")]
	public bool? Consumed { get; set; }
}
