namespace DiscoSdk.Models.Activities;

/// <summary>
/// Read-only view of <see cref="ActivityParty"/>. Discord reports party size as a two-element
/// integer array (<c>[current, max]</c>); the wrapper splits it into named properties.
/// </summary>
public interface IActivityParty
{
    /// <summary>The party id, or <c>null</c> when the activity has no party identifier.</summary>
    string? Id { get; }

    /// <summary>The current party size, or <c>null</c> when Discord did not report it.</summary>
    int? CurrentSize { get; }

    /// <summary>The maximum party size, or <c>null</c> when Discord did not report it.</summary>
    int? MaxSize { get; }
}
