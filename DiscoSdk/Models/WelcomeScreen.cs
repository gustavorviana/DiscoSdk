using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord guild welcome screen.
/// </summary>
public class WelcomeScreen
{
	/// <summary>
	/// Gets or sets the server description shown in the welcome screen.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the channels shown in the welcome screen, up to 5.
	/// </summary>
	[JsonPropertyName("welcome_channels")]
	public WelcomeScreenChannel[]? WelcomeChannels { get; set; }
}