using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>POST /guilds/{guild.id}/scheduled-events</c>. The factory supplies the
/// required <c>name</c>, <c>scheduled_start_time</c>, and <c>entity_type</c>; the optional fields
/// are set through the chain.
/// </summary>
public interface ICreateScheduledEventAction : IRestAction<IGuildScheduledEvent>
{
	/// <summary>Sets the privacy level. Defaults to <see cref="ScheduledEventPrivacyLevel.GuildOnly"/>.</summary>
	ICreateScheduledEventAction SetPrivacyLevel(ScheduledEventPrivacyLevel privacyLevel);

	/// <summary>The stage / voice channel the event runs in. Required for Stage and Voice events.</summary>
	ICreateScheduledEventAction SetChannel(Snowflake channelId);

	/// <summary>Sets the description (0-1000 chars).</summary>
	ICreateScheduledEventAction SetDescription(string description);

	/// <summary>Sets when the event ends. Required for External events; optional for Stage / Voice.</summary>
	ICreateScheduledEventAction SetScheduledEndTime(DateTimeOffset scheduledEndTime);

	/// <summary>
	/// Sets the external location (sets <c>entity_metadata.location</c>). Required for External events.
	/// Mutually exclusive with <see cref="SetChannel"/>.
	/// </summary>
	ICreateScheduledEventAction SetLocation(string location);

	/// <summary>Sets the cover image as a data URI (PNG/JPEG/GIF).</summary>
	ICreateScheduledEventAction SetImage(string imageDataUri);
}
