using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a banned user in a Discord guild.
/// </summary>
public class Ban
{
	/// <summary>
	/// Gets or sets the reason for the ban, if any.
	/// </summary>
	[JsonPropertyName("reason")]
	public string? Reason { get; set; }

	/// <summary>
	/// Gets or sets the user who was banned.
	/// </summary>
	[JsonPropertyName("user")]
	public User User { get; set; } = default!;
}

