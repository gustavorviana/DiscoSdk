using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a member in a guild widget.
/// </summary>
public class GuildWidgetMember
{
	/// <summary>
	/// Gets or sets the user's unique identifier.
	/// </summary>
	[JsonPropertyName("id")]
	public Snowflake? Id { get; set; }

	/// <summary>
	/// Gets or sets the user's username.
	/// </summary>
	[JsonPropertyName("username")]
	public string Username { get; set; } = default!;

	/// <summary>
	/// Gets or sets the user's discriminator.
	/// </summary>
	[JsonPropertyName("discriminator")]
	public string Discriminator { get; set; } = default!;

	/// <summary>
	/// Gets or sets the user's avatar hash.
	/// </summary>
	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	/// <summary>
	/// Gets or sets the user's status.
	/// </summary>
	[JsonPropertyName("status")]
	public OnlineStatus Status { get; set; }

	/// <summary>
	/// Gets or sets the user's avatar URL.
	/// </summary>
	[JsonPropertyName("avatar_url")]
	public string? AvatarUrl { get; set; }
}

