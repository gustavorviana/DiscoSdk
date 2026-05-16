namespace DiscoSdk.Models;

/// <summary>
/// Read-only view of the audit log returned by <c>GET /guilds/{guild.id}/audit-logs</c>.
/// </summary>
public interface IAuditLog
{
	/// <summary>The audit log entries, in reverse-chronological order.</summary>
	IReadOnlyList<IAuditLogEntry> AuditLogEntries { get; }
}
