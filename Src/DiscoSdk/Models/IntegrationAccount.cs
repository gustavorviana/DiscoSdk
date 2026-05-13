using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// A reference to the external account a Discord integration belongs to (e.g. a Twitch or YouTube account).
/// </summary>
public class IntegrationAccount : IIntegrationAccount
{
	/// <summary>The ID of the external account.</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;

	/// <summary>The display name of the external account.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;
}