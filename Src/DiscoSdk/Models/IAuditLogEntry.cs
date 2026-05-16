using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models;

/// <summary>
/// Read-only view of a single audit log entry.
/// </summary>
public interface IAuditLogEntry
{
	/// <summary>The id of the entry (also a snowflake whose timestamp matches the entry time).</summary>
	Snowflake Id { get; }

	/// <summary>The id of the affected entity (webhook, user, role, etc.) or <c>null</c>.</summary>
	string? TargetId { get; }

	/// <summary>The id of the user who performed the action, or <c>null</c> for system actions.</summary>
	Snowflake? UserId { get; }

	/// <summary>The audit log action type.</summary>
	AuditLogActionType ActionType { get; }

	/// <summary>The recorded property changes, or <c>null</c> when the action carries none.</summary>
	IReadOnlyList<IAuditLogChange>? Changes { get; }

	/// <summary>Action-type-specific extra context (pruned count, channel id, etc.).</summary>
	IAuditLogOptions? Options { get; }

	/// <summary>Optional reason text supplied by the moderator via <c>X-Audit-Log-Reason</c>.</summary>
	string? Reason { get; }
}
