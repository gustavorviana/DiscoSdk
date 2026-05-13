using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// Read-only view of a member of a Discord developer <see cref="ITeam"/>.
/// </summary>
public interface ITeamMember
{
	/// <summary>The user's membership state on the team.</summary>
	TeamMembershipState MembershipState { get; }

	/// <summary>The ID of the parent team.</summary>
	Snowflake TeamId { get; }

	/// <summary>The team member's user.</summary>
	IUser User { get; }

	/// <summary>The member's role on the team (<c>owner</c>, <c>admin</c>, <c>developer</c> or <c>read_only</c>).</summary>
	string Role { get; }
}
