using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IAuditLogEntry"/>
internal class AuditLogEntry : IAuditLogEntry
{
	[JsonPropertyName("id")]
	public Snowflake Id { get; init; } = default!;

	[JsonPropertyName("target_id")]
	public string? TargetId { get; init; }

	[JsonPropertyName("user_id")]
	public Snowflake? UserId { get; init; }

	[JsonPropertyName("action_type")]
	public AuditLogActionType ActionType { get; init; }

	[JsonPropertyName("changes")]
	public AuditLogChange[]? Changes { get; init; }

	[JsonPropertyName("options")]
	public AuditLogOptions? Options { get; init; }

	[JsonPropertyName("reason")]
	public string? Reason { get; init; }

	IReadOnlyList<IAuditLogChange>? IAuditLogEntry.Changes => Changes;
	IAuditLogOptions? IAuditLogEntry.Options => Options;
}
