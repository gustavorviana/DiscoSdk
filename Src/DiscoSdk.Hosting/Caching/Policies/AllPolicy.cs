using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>Caches every member observed by the gateway.</summary>
internal sealed class AllPolicy : IMemberCachePolicy
{
    public static AllPolicy Instance { get; } = new();

    private AllPolicy() { }

    public bool ShouldCache(IMember member) => true;
}
