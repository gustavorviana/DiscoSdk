namespace DiscoSdk.Models.Enums;

/// <summary>
/// Where an application can be installed (its supported installation contexts).
/// </summary>
public enum ApplicationIntegrationType
{
	/// <summary>The app is installable to guilds.</summary>
	GuildInstall = 0,

	/// <summary>The app is installable to individual users.</summary>
	UserInstall = 1
}
