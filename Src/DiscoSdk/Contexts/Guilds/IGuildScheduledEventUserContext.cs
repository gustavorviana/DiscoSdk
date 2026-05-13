using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>GUILD_SCHEDULED_EVENT_USER_ADD</c> and <c>_USER_REMOVE</c> Gateway events.
/// </summary>
public interface IGuildScheduledEventUserContext : IContext
{
	/// <summary>ID of the scheduled event.</summary>
	Snowflake ScheduledEventId { get; }

	/// <summary>ID of the user that subscribed/unsubscribed.</summary>
	Snowflake UserId { get; }

	/// <summary>The guild hosting the scheduled event.</summary>
	IGuild Guild { get; }
}
