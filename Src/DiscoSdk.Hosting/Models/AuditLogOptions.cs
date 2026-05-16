using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IAuditLogOptions"/>
internal class AuditLogOptions : IAuditLogOptions
{
	[JsonPropertyName("delete_member_days")]
	public string? DeleteMemberDays { get; init; }

	[JsonPropertyName("members_removed")]
	public string? MembersRemoved { get; init; }

	[JsonPropertyName("channel_id")]
	public Snowflake? ChannelId { get; init; }

	[JsonPropertyName("count")]
	public string? Count { get; init; }

	[JsonPropertyName("id")]
	public Snowflake? Id { get; init; }

	[JsonPropertyName("type")]
	public string? Type { get; init; }

	[JsonPropertyName("role_name")]
	public string? RoleName { get; init; }
}
