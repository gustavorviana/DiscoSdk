namespace DiscoSdk.Models.Applications;

/// <summary>
/// Read-only view of the per-installation-context configuration on an <see cref="IApplication"/>.
/// </summary>
public interface IApplicationIntegrationTypeConfiguration
{
	/// <summary>Install parameters for this installation context.</summary>
	IApplicationInstallParams? Oauth2InstallParams { get; }
}
