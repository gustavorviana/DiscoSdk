using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord voice region.
/// </summary>
public class VoiceRegion
{
	/// <summary>
	/// Gets or sets the unique identifier for the region.
	/// </summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;

	/// <summary>
	/// Gets or sets the name of the region.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets a value indicating whether this is the optimal region for the guild.
	/// </summary>
	[JsonPropertyName("optimal")]
	public bool Optimal { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this region is deprecated.
	/// </summary>
	[JsonPropertyName("deprecated")]
	public bool Deprecated { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is a custom region (used for events/etc).
	/// </summary>
	[JsonPropertyName("custom")]
	public bool Custom { get; set; }
}

