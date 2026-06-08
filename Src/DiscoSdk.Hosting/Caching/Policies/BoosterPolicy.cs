using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>Caches only members currently boosting the guild.</summary>
internal sealed class BoosterPolicy : IMemberCachePolicy
{
    public static BoosterPolicy Instance { get; } = new();

    private BoosterPolicy() { }

    public bool ShouldCache(IMember member) => member.IsBoosting;
}
