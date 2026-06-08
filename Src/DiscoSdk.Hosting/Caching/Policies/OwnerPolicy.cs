using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>Caches only the guild owner.</summary>
internal sealed class OwnerPolicy : IMemberCachePolicy
{
    public static OwnerPolicy Instance { get; } = new();

    private OwnerPolicy() { }

    public bool ShouldCache(IMember member) => member.IsOwner;
}
