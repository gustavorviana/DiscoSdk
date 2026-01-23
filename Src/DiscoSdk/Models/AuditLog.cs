using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents the response from Discord's audit log API.
/// </summary>
public class AuditLog
{
	/// <summary>
	/// Gets or sets the audit log entries.
	/// </summary>
	[JsonPropertyName("audit_log_entries")]
	public AuditLogEntry[] AuditLogEntries { get; set; } = [];
}

