namespace DiscoSdk.Models.Activities;

/// <summary>
/// Read-only view of <see cref="ActivityTimestamps"/>. Discord reports the start/end as Unix
/// epoch milliseconds; the wrapper exposes them as <see cref="DateTimeOffset"/> values for
/// ergonomic consumption.
/// </summary>
public interface IActivityTimestamps
{
    /// <summary>When the activity started, or <c>null</c> when Discord did not report it.</summary>
    DateTimeOffset? Start { get; }

    /// <summary>When the activity ends (or is expected to end), or <c>null</c> when Discord did not report it.</summary>
    DateTimeOffset? End { get; }
}
