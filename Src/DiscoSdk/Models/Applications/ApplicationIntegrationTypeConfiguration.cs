using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// Per-installation-context configuration for an application (keyed by <c>ApplicationIntegrationType</c>
/// in <see cref="Application.IntegrationTypesConfig"/>). Doubles as the public read surface
/// (<see cref="IApplicationIntegrationTypeConfiguration"/>).
/// </summary>
public class ApplicationIntegrationTypeConfiguration : IApplicationIntegrationTypeConfiguration
{
	/// <summary>Install parameters for this installation context.</summary>
	[JsonPropertyName("oauth2_install_params")]
	public ApplicationInstallParams? Oauth2InstallParams { get; set; }

	IApplicationInstallParams? IApplicationIntegrationTypeConfiguration.Oauth2InstallParams => Oauth2InstallParams;
}
