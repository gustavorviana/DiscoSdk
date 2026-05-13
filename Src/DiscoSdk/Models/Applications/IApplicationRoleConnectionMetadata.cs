using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// Read-only view of a single application role-connection metadata record.
/// </summary>
public interface IApplicationRoleConnectionMetadata
{
	/// <summary>The type of comparison applied to this metadata value.</summary>
	ApplicationRoleConnectionMetadataType Type { get; }

	/// <summary>Dictionary key for the metadata field.</summary>
	string Key { get; }

	/// <summary>The metadata field's name.</summary>
	string Name { get; }

	/// <summary>Localizations for the name.</summary>
	IReadOnlyDictionary<string, string>? NameLocalizations { get; }

	/// <summary>The metadata field's description.</summary>
	string Description { get; }

	/// <summary>Localizations for the description.</summary>
	IReadOnlyDictionary<string, string>? DescriptionLocalizations { get; }
}
