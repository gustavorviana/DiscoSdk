using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a preview of a Discord guild (for public guilds).
/// </summary>
public class GuildPreview
{
	/// <summary>
	/// Gets or sets the guild's unique identifier.
	/// </summary>
	[JsonPropertyName("id")]
	public DiscordId Id { get; set; } = default!;

	/// <summary>
	/// Gets or sets the guild's name.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets the guild's icon hash.
	/// </summary>
	[JsonPropertyName("icon")]
	public string? Icon { get; set; }

	/// <summary>
	/// Gets or sets the guild's splash hash.
	/// </summary>
	[JsonPropertyName("splash")]
	public string? Splash { get; set; }

	/// <summary>
	/// Gets or sets the guild's discovery splash hash.
	/// </summary>
	[JsonPropertyName("discovery_splash")]
	public string? DiscoverySplash { get; set; }

	/// <summary>
	/// Gets or sets the guild's emojis.
	/// </summary>
	[JsonPropertyName("emojis")]
	public Emoji[]? Emojis { get; set; }

	/// <summary>
	/// Gets or sets the guild's enabled features.
	/// </summary>
	[JsonPropertyName("features")]
	public string[]? Features { get; set; }

	/// <summary>
	/// Gets or sets the approximate number of members in the guild.
	/// </summary>
	[JsonPropertyName("approximate_member_count")]
	public int ApproximateMemberCount { get; set; }

	/// <summary>
	/// Gets or sets the approximate number of online members in the guild.
	/// </summary>
	[JsonPropertyName("approximate_presence_count")]
	public int ApproximatePresenceCount { get; set; }

	/// <summary>
	/// Gets or sets the guild's description.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}

