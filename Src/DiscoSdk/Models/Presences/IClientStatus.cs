namespace DiscoSdk.Models.Presences;

/// <summary>
/// Per-client-platform online status for a user. Each property is the user's status on that
/// platform, or <c>null</c> if not signed in there. Values follow the Discord status string:
/// <c>"online"</c>, <c>"idle"</c>, <c>"dnd"</c>.
/// </summary>
public interface IClientStatus
{
	/// <summary>Status on the desktop client, or <c>null</c> if not signed in on desktop.</summary>
	string? Desktop { get; }

	/// <summary>Status on the mobile client, or <c>null</c> if not signed in on mobile.</summary>
	string? Mobile { get; }

	/// <summary>Status on the web client, or <c>null</c> if not signed in on web.</summary>
	string? Web { get; }
}
