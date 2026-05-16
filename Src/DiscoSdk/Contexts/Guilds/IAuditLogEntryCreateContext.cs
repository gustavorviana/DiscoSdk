using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_AUDIT_LOG_ENTRY_CREATE</c> Gateway event — a new audit log entry was
/// written for the guild (requires the VIEW_AUDIT_LOG permission and the GUILD_MODERATION intent).
/// </summary>
public interface IAuditLogEntryCreateContext : IContext
{
	/// <summary>The new audit log entry.</summary>
	IAuditLogEntry Entry { get; }

	/// <summary>The guild the entry belongs to.</summary>
	IGuild Guild { get; }
}
