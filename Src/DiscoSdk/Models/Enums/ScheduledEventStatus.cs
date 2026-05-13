namespace DiscoSdk.Models.Enums;

/// <summary>
/// Lifecycle status of a guild scheduled event.
/// </summary>
public enum ScheduledEventStatus
{
	/// <summary>The event has not started yet.</summary>
	Scheduled = 1,

	/// <summary>The event is currently in progress.</summary>
	Active = 2,

	/// <summary>The event has ended (terminal state).</summary>
	Completed = 3,

	/// <summary>The event was canceled before it started (terminal state).</summary>
	Canceled = 4,
}
