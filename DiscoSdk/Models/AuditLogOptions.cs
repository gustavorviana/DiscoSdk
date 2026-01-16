using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents additional options for certain audit log action types.
/// </summary>
public class AuditLogOptions
{
	/// <summary>
	/// Gets or sets the number of days after which inactive members were pruned.
	/// </summary>
	[JsonPropertyName("delete_member_days")]
	public string? DeleteMemberDays { get; set; }

	/// <summary>
	/// Gets or sets the number of members removed by the prune.
	/// </summary>
	[JsonPropertyName("members_removed")]
	public string? MembersRemoved { get; set; }

	/// <summary>
	/// Gets or sets the channel in which entities were targeted.
	/// </summary>
	[JsonPropertyName("channel_id")]
	public Snowflake? ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the number of entities that were targeted.
	/// </summary>
	[JsonPropertyName("count")]
	public string? Count { get; set; }

	/// <summary>
	/// Gets or sets the ID of the overwritten entity.
	/// </summary>
	[JsonPropertyName("id")]
	public Snowflake? Id { get; set; }

	/// <summary>
	/// Gets or sets the type of overwritten entity ("member" or "role").
	/// </summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>
	/// Gets or sets the name of the role if type is "role".
	/// </summary>
	[JsonPropertyName("role_name")]
	public string? RoleName { get; set; }
}

