namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents the auto-archive duration for threads.
/// </summary>
public enum ThreadAutoArchiveDuration
{
	/// <summary>
	/// Thread will archive after 1 hour of inactivity.
	/// </summary>
	OneHour = 60,

	/// <summary>
	/// Thread will archive after 24 hours of inactivity.
	/// </summary>
	OneDay = 1440,

	/// <summary>
	/// Thread will archive after 3 days of inactivity.
	/// </summary>
	ThreeDays = 4320,

	/// <summary>
	/// Thread will archive after 7 days of inactivity.
	/// </summary>
	OneWeek = 10080
}

