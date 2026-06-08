using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>Caches only members that have not yet completed membership screening.</summary>
internal sealed class PendingPolicy : IMemberCachePolicy
{
    public static PendingPolicy Instance { get; } = new();

    private PendingPolicy() { }

    public bool ShouldCache(IMember member) => member.IsPending;
}
