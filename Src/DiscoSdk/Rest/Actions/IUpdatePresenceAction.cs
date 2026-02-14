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
	/// Sets the single activity for the presence update. Use for editing/updating; <see cref="Activity"/> is read-only model.
	/// </summary>
	/// <param name="activity">The activity update object, or null to clear.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction SetActivity(ActivityUpdate? activity);

	/// <summary>
	/// Sets the activities for the presence update.
	/// </summary>
	/// <param name="activities">The activity updates to set, or null to clear all.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction SetActivities(params ActivityUpdate[] activities);

	/// <summary>
	/// Adds an activity to the presence (for update).
	/// </summary>
	/// <param name="activity">The activity update to add.</param>
	/// <returns>The current <see cref="IUpdatePresenceAction"/> instance.</returns>
	IUpdatePresenceAction AddActivity(ActivityUpdate activity);

	/// <summary>
	/// Adds an activity using a builder that produces an <see cref="ActivityUpdate"/>.
	/// </summary>
	/// <param name="activity">The activity builder.</param>
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