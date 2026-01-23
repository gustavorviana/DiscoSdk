using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a channel in a guild widget.
/// </summary>
public class GuildWidgetChannel
{
	/// <summary>
	/// Gets or sets the channel's unique identifier.
	/// </summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>
	/// Gets or sets the channel's name.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets the channel's position.
	/// </summary>
	[JsonPropertyName("position")]
	public int Position { get; set; }
}

