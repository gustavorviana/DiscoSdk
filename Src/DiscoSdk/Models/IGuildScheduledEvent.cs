using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Public read/action surface for a Discord guild scheduled event.
/// </summary>
public interface IGuildScheduledEvent
{
	/// <summary>The scheduled event id.</summary>
	Snowflake Id { get; }

	/// <summary>The guild this event belongs to.</summary>
	Snowflake GuildId { get; }

	/// <summary>Channel id for stage/voice events, null for external events.</summary>
	Snowflake? ChannelId { get; }

	/// <summary>The user that created the event, if known.</summary>
	Snowflake? CreatorId { get; }

	/// <summary>The user that created the event, if Discord supplied the full user object.</summary>
	IUser? Creator { get; }

	/// <summary>Event name (1-100 chars).</summary>
	string Name { get; }

	/// <summary>Event description (0-1000 chars).</summary>
	string? Description { get; }

	/// <summary>When the event starts.</summary>
	DateTimeOffset ScheduledStartTime { get; }

	/// <summary>When the event ends. Required for External events.</summary>
	DateTimeOffset? ScheduledEndTime { get; }

	/// <summary>Privacy level.</summary>
	ScheduledEventPrivacyLevel PrivacyLevel { get; }

	/// <summary>Lifecycle status.</summary>
	ScheduledEventStatus Status { get; }

	/// <summary>Venue type.</summary>
	ScheduledEventEntityType EntityType { get; }

	/// <summary>If the event is bound to a stage instance, its id.</summary>
	Snowflake? EntityId { get; }

	/// <summary>External venue location (only set for External events).</summary>
	string? Location { get; }

	/// <summary>Number of users interested in the event (only present when requested).</summary>
	int? UserCount { get; }

	/// <summary>Cover image hash.</summary>
	string? Image { get; }

	/// <summary>
	/// Modifies the event. Returns a fluent builder — chain <c>SetName</c> / <c>SetStatus</c> /
	/// <c>SetScheduledStartTime</c> / etc. and finish with <c>ExecuteAsync</c>. Only the fields
	/// you touch are sent on the wire.
	/// </summary>
	IModifyScheduledEventAction Modify();

	/// <summary>Deletes (cancels) the event.</summary>
	IRestAction Delete();

	/// <summary>
	/// Lists users that subscribed to this event. <paramref name="limit"/> caps the page at 100.
	/// Use <paramref name="before"/>/<paramref name="after"/> for pagination by user id.
	/// </summary>
	IRestAction<IReadOnlyList<IGuildScheduledEventUser>> GetUsers(
		int? limit = null,
		bool? withMember = null,
		Snowflake? before = null,
		Snowflake? after = null);
}
