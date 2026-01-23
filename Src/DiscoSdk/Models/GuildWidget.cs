using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord guild widget.
/// </summary>
public class GuildWidget
{
	/// <summary>
	/// Gets or sets the guild's unique identifier.
	/// </summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>
	/// Gets or sets the guild's name.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets the instant invite URL for the guild's specified widget invite channel.
	/// </summary>
	[JsonPropertyName("instant_invite")]
	public string? InstantInvite { get; set; }

	/// <summary>
	/// Gets or sets the voice and stage channels which are accessible by @everyone.
	/// </summary>
	[JsonPropertyName("channels")]
	public GuildWidgetChannel[]? Channels { get; set; }

	/// <summary>
	/// Gets or sets the special widget user objects that includes users present in the widget.
	/// </summary>
	[JsonPropertyName("members")]
	public GuildWidgetMember[]? Members { get; set; }

	/// <summary>
	/// Gets or sets the number of online members in the guild.
	/// </summary>
	[JsonPropertyName("presence_count")]
	public int PresenceCount { get; set; }
}

