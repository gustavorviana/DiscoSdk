using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// The default scopes and permissions used when adding an application to a guild via its install link.
/// Doubles as the public read surface (<see cref="IApplicationInstallParams"/>).
/// </summary>
public class ApplicationInstallParams : IApplicationInstallParams
{
	/// <summary>The OAuth2 scopes to request.</summary>
	[JsonPropertyName("scopes")]
	public string[] Scopes { get; set; } = [];

	/// <summary>The permissions bitfield (as a string) to request for the bot role.</summary>
	[JsonPropertyName("permissions")]
	public string Permissions { get; set; } = default!;

	IReadOnlyList<string> IApplicationInstallParams.Scopes => Scopes;
}
