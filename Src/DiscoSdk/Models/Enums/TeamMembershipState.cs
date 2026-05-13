namespace DiscoSdk.Models.Enums;

/// <summary>
/// The state of a member's invitation to a Discord developer team.
/// </summary>
public enum TeamMembershipState
{
	/// <summary>The user has been invited but has not yet accepted.</summary>
	Invited = 1,

	/// <summary>The user has accepted the invitation.</summary>
	Accepted = 2
}
