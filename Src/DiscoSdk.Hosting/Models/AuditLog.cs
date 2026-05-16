using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IAuditLog"/>
internal class AuditLog : IAuditLog
{
	[JsonPropertyName("audit_log_entries")]
	public AuditLogEntry[] AuditLogEntries { get; init; } = [];

	IReadOnlyList<IAuditLogEntry> IAuditLog.AuditLogEntries => AuditLogEntries;
}
