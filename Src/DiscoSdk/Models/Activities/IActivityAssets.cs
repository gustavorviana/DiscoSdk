namespace DiscoSdk.Models.Activities;

/// <summary>
/// Read-only view of <see cref="ActivityAssets"/> — the Rich Presence imagery keys and hover
/// labels broadcast with an activity.
/// </summary>
public interface IActivityAssets
{
    /// <summary>The large image asset key, or <c>null</c> when none is set.</summary>
    string? LargeImage { get; }

    /// <summary>The hover label shown on the large image, or <c>null</c> when none is set.</summary>
    string? LargeText { get; }

    /// <summary>The small image asset key, or <c>null</c> when none is set.</summary>
    string? SmallImage { get; }

    /// <summary>The hover label shown on the small image, or <c>null</c> when none is set.</summary>
    string? SmallText { get; }
}
