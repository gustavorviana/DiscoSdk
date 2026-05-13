using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>GUILD_SCHEDULED_EVENT_CREATE</c>, <c>_UPDATE</c>, and <c>_DELETE</c>.
/// </summary>
public interface IGuildScheduledEventContext : IContext
{
	/// <summary>The scheduled event, wrapped with REST actions (Modify/Delete/GetUsers).</summary>
	IGuildScheduledEvent ScheduledEvent { get; }

	/// <summary>The guild hosting the event.</summary>
	IGuild Guild { get; }
}
