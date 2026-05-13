namespace DiscoSdk.Models.Applications;

/// <summary>
/// Read view of the current user's role-connection data for a specific application.
/// </summary>
public interface IApplicationRoleConnection
{
	/// <summary>The vanity name of the platform a bot has connected.</summary>
	string? PlatformName { get; }

	/// <summary>The username on the platform a bot has connected.</summary>
	string? PlatformUsername { get; }

	/// <summary>The metadata keys and values supplied by the bot for this user.</summary>
	IReadOnlyDictionary<string, string>? Metadata { get; }
}
