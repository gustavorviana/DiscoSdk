namespace DiscoSdk.Models;

/// <summary>
/// Read-only snapshot of a guild's incident-actions state — when invites and/or DMs are
/// temporarily suspended.
/// </summary>
public interface IIncidentsData
{
	/// <summary>Timestamp until which invites are suspended, or <c>null</c> if not suspended.</summary>
	DateTimeOffset? InvitesDisabledUntil { get; }

	/// <summary>Timestamp until which DMs are suspended, or <c>null</c> if not suspended.</summary>
	DateTimeOffset? DmsDisabledUntil { get; }
}
