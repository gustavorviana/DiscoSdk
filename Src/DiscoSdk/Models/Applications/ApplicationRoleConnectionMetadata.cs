using DiscoSdk.Models.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// A single record describing a piece of metadata an application can attach to a user's connected
/// account, which guilds can then use as a linked-roles requirement. Doubles as the public read
/// surface (<see cref="IApplicationRoleConnectionMetadata"/>).
/// </summary>
public class ApplicationRoleConnectionMetadata : IApplicationRoleConnectionMetadata
{
	/// <summary>The type of comparison applied to this metadata value.</summary>
	[JsonPropertyName("type")]
	public ApplicationRoleConnectionMetadataType Type { get; set; }

	/// <summary>Dictionary key for the metadata field (a-z, 0-9, or _; 1-50 chars).</summary>
	[JsonPropertyName("key")]
	public string Key { get; set; } = default!;

	/// <summary>The metadata field's name (1-100 chars).</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>Localizations for the name.</summary>
	[JsonPropertyName("name_localizations")]
	public Dictionary<string, string>? NameLocalizations { get; set; }

	/// <summary>The metadata field's description (1-200 chars).</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = default!;

	/// <summary>Localizations for the description.</summary>
	[JsonPropertyName("description_localizations")]
	public Dictionary<string, string>? DescriptionLocalizations { get; set; }

	IReadOnlyDictionary<string, string>? IApplicationRoleConnectionMetadata.NameLocalizations => NameLocalizations;
	IReadOnlyDictionary<string, string>? IApplicationRoleConnectionMetadata.DescriptionLocalizations => DescriptionLocalizations;
}
