using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Monetization;

/// <summary>
/// Represents a Discord SKU — a premium offering (one-time purchase or subscription) that can be made
/// available to an application's users.
/// </summary>
public class Sku
{
	/// <summary>The ID of the SKU.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>The type of SKU.</summary>
	[JsonPropertyName("type")]
	public SkuType Type { get; set; }

	/// <summary>The ID of the parent application.</summary>
	[JsonPropertyName("application_id")]
	public Snowflake ApplicationId { get; set; } = default!;

	/// <summary>The customer-facing name of the premium offering.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The system-generated URL slug based on the SKU's name.</summary>
	[JsonPropertyName("slug")]
	public string Slug { get; set; } = default!;

	/// <summary>Flags describing how the SKU can be purchased.</summary>
	[JsonPropertyName("flags")]
	public SkuFlags Flags { get; set; }
}
