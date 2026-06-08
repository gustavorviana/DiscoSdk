using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild scheduled-event surface — every operation that targets
/// <c>/guilds/:id/scheduled-events*</c>.
/// </summary>
public interface IGuildScheduledEvents
{
    /// <summary>Builds a deferred REST action that lists this guild's scheduled events.</summary>
    /// <param name="withUserCount">If <c>true</c>, each event includes its <c>UserCount</c>.</param>
    IRestAction<IReadOnlyList<IGuildScheduledEvent>> GetAll(bool? withUserCount = null);

    /// <summary>Builds a deferred REST action that retrieves a single scheduled event by id.</summary>
    IRestAction<IGuildScheduledEvent> Get(Snowflake eventId, bool? withUserCount = null);

    /// <summary>
    /// Builds a deferred fluent action that creates a scheduled event. For Stage/Voice events chain
    /// <c>SetChannel(...)</c>; for External events chain <c>SetLocation(...).SetScheduledEndTime(...)</c>.
    /// </summary>
    /// <param name="name">Event name (1-100 chars).</param>
    /// <param name="scheduledStartTime">When the event starts.</param>
    /// <param name="entityType">Venue type (Stage / Voice / External).</param>
    ICreateScheduledEventAction Create(string name, DateTimeOffset scheduledStartTime, ScheduledEventEntityType entityType);
}
