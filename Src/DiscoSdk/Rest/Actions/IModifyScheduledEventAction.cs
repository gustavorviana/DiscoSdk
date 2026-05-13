using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>PATCH /guilds/{guild.id}/scheduled-events/{event.id}</c>. Every setter is
/// optional — only the fields you touch are sent on the wire.
/// </summary>
public interface IModifyScheduledEventAction : IRestAction<IGuildScheduledEvent>
{
	/// <summary>Renames the event.</summary>
	IModifyScheduledEventAction SetName(string name);

	/// <summary>Updates the description.</summary>
	IModifyScheduledEventAction SetDescription(string description);

	/// <summary>Reschedules the start.</summary>
	IModifyScheduledEventAction SetScheduledStartTime(DateTimeOffset scheduledStartTime);

	/// <summary>Reschedules the end. Required when entity type is External.</summary>
	IModifyScheduledEventAction SetScheduledEndTime(DateTimeOffset scheduledEndTime);

	/// <summary>Updates the privacy level.</summary>
	IModifyScheduledEventAction SetPrivacyLevel(ScheduledEventPrivacyLevel privacyLevel);

	/// <summary>
	/// Transitions the lifecycle status. Allowed transitions: Scheduled → Active, Active → Completed,
	/// Scheduled → Canceled. Discord rejects illegal transitions.
	/// </summary>
	IModifyScheduledEventAction SetStatus(ScheduledEventStatus status);

	/// <summary>Changes the venue type.</summary>
	IModifyScheduledEventAction SetEntityType(ScheduledEventEntityType entityType);

	/// <summary>Reassigns to a different stage/voice channel.</summary>
	IModifyScheduledEventAction SetChannel(Snowflake channelId);

	/// <summary>Updates the external location (sets <c>entity_metadata.location</c>).</summary>
	IModifyScheduledEventAction SetLocation(string location);

	/// <summary>Updates the cover image (data URI).</summary>
	IModifyScheduledEventAction SetImage(string imageDataUri);
}
