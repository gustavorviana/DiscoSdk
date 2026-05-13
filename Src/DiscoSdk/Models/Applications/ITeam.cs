namespace DiscoSdk.Models.Applications;

/// <summary>
/// Read-only view of a Discord developer team.
/// </summary>
public interface ITeam
{
	/// <summary>The unique ID of the team.</summary>
	Snowflake Id { get; }

	/// <summary>A hash of the team's icon image.</summary>
	string? Icon { get; }

	/// <summary>The name of the team.</summary>
	string Name { get; }

	/// <summary>The user ID of the current team owner.</summary>
	Snowflake OwnerUserId { get; }

	/// <summary>The members of the team.</summary>
	IReadOnlyList<ITeamMember> Members { get; }
}
