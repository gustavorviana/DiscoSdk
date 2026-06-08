using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>Rejects every member. Every lookup goes through REST.</summary>
internal sealed class NonePolicy : IMemberCachePolicy
{
    public static NonePolicy Instance { get; } = new();

    private NonePolicy() { }

    public bool ShouldCache(IMember member) => false;
}
