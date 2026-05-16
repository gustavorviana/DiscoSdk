using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Bot/OAuth2 application reference attached to a Discord integration.
/// </summary>
internal class IntegrationApplication
{
	/// <summary>The ID of the app.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; }

	/// <summary>The name of the app.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The icon hash of the app, if any.</summary>
	[JsonPropertyName("icon")]
	public string? Icon { get; set; }

	/// <summary>The description of the app.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = default!;

	/// <summary>The bot user associated with this app, if any.</summary>
	[JsonPropertyName("bot")]
	public User? Bot { get; set; }
}
