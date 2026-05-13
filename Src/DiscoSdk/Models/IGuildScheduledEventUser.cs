namespace DiscoSdk.Models;

/// <summary>
/// A user that subscribed to a guild scheduled event.
/// </summary>
public interface IGuildScheduledEventUser
{
	/// <summary>The scheduled event this subscription is for.</summary>
	Snowflake ScheduledEventId { get; }

	/// <summary>The user that subscribed.</summary>
	IUser User { get; }

	/// <summary>The user's guild member object — only populated when the caller requested <c>with_member=true</c>.</summary>
	IMember? Member { get; }
}
