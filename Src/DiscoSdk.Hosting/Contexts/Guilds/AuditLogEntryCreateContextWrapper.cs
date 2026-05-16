using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class AuditLogEntryCreateContextWrapper(DiscordClient client, AuditLogEntry entry, IGuild guild)
	: ContextWrapper(client), IAuditLogEntryCreateContext
{
	public IAuditLogEntry Entry => entry;
	public IGuild Guild => guild;
}
