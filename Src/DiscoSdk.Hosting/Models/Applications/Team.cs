using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// Represents a Discord developer team — a group that collectively owns one or more applications.
/// </summary>
internal class Team
{
	/// <summary>The unique ID of the team.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>A hash of the team's icon image.</summary>
	[JsonPropertyName("icon")]
	public string? Icon { get; set; }

	/// <summary>The name of the team.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The user ID of the current team owner.</summary>
	[JsonPropertyName("owner_user_id")]
	public Snowflake OwnerUserId { get; set; } = default!;

	/// <summary>The members of the team.</summary>
	[JsonPropertyName("members")]
	public TeamMember[] Members { get; set; } = [];
}
