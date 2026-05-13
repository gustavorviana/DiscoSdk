namespace DiscoSdk.Models.Applications;

/// <summary>
/// Read-only view of the default scopes and permissions used when adding an application to a guild.
/// </summary>
public interface IApplicationInstallParams
{
	/// <summary>The OAuth2 scopes to request.</summary>
	IReadOnlyList<string> Scopes { get; }

	/// <summary>The permissions bitfield (as a string) to request for the bot role.</summary>
	string Permissions { get; }
}
