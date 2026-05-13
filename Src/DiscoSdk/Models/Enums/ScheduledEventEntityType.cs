namespace DiscoSdk.Models.Enums;

/// <summary>
/// What kind of "venue" a scheduled event takes place in.
/// </summary>
public enum ScheduledEventEntityType
{
	/// <summary>Event runs in a stage channel.</summary>
	StageInstance = 1,

	/// <summary>Event runs in a voice channel.</summary>
	Voice = 2,

	/// <summary>Event runs at an external location (e.g. a physical address or external URL).</summary>
	External = 3,
}
