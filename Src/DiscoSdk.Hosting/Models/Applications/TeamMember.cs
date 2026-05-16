using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// A member of a Discord developer <see cref="Team"/>.
/// </summary>
internal class TeamMember
{
	/// <summary>The user's membership state on the team.</summary>
	[JsonPropertyName("membership_state")]
	public TeamMembershipState MembershipState { get; set; }

	/// <summary>The ID of the parent team.</summary>
	[JsonPropertyName("team_id")]
	public Snowflake TeamId { get; set; } = default!;

	/// <summary>The avatar, discriminator, ID and username of the user.</summary>
	[JsonPropertyName("user")]
	public User User { get; set; } = default!;

	/// <summary>The member's role on the team (<c>owner</c>, <c>admin</c>, <c>developer</c> or <c>read_only</c>).</summary>
	[JsonPropertyName("role")]
	public string Role { get; set; } = default!;
}
