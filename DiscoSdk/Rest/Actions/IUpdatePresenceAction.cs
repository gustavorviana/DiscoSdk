using DiscoSdk.Models;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for updating the bot's presence.
/// </summary>
public interface IUpdatePresenceAction : IRestAction
{
	/// <summary>
	/// Sets the online status.
	/// </summary>
	/// <param name="status">The online status to set.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction SetStatus(OnlineStatus status);

	/// <summary>
	/// Sets the activities.
	/// </summary>
	/// <param name="activities">The activities to set, or null to clear all activities.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction SetActivities(Activity[]? activities);

	/// <summary>
	/// Adds an activity to the presence.
	/// </summary>
	/// <param name="activity">The activity to add.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction AddActivity(Activity activity);

	/// <summary>
	/// Adds an activity to the presence using an activity builder.
	/// </summary>
	/// <param name="activity">The activity builder to add.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction AddActivity(IActivity activity);

	/// <summary>
	/// Sets whether the bot is AFK.
	/// </summary>
	/// <param name="afk">True if the bot is AFK, false otherwise.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction SetAfk(bool afk);

	/// <summary>
	/// Sets the timestamp since when the status changed.
	/// </summary>
	/// <param name="since">The timestamp, or null to use the current time.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction SetSince(DateTimeOffset? since);

	/// <summary>
	/// Clears all activities.
	/// </summary>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction ClearActivities();
}