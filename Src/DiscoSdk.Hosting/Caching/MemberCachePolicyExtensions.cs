using DiscoSdk.Caching;
using DiscoSdk.Hosting.Caching.Policies;

namespace DiscoSdk.Hosting.Caching;

/// <summary>
/// Bridges between the user-facing <see cref="MemberCachePolicy"/> preset enum and the internal
/// <see cref="IMemberCachePolicy"/> instances consumed by the runtime.
/// </summary>
public static class MemberCachePolicyExtensions
{
    /// <summary>
    /// Resolves a preset to its concrete policy implementation.
    /// </summary>
    /// <param name="preset">The preset to resolve.</param>
    /// <returns>The matching <see cref="IMemberCachePolicy"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The supplied value is not a known preset.</exception>
    public static IMemberCachePolicy ToPolicy(this MemberCachePolicy preset) => preset switch
    {
        MemberCachePolicy.None   => NonePolicy.Instance,
        MemberCachePolicy.All    => AllPolicy.Instance,
        MemberCachePolicy.Owner  => OwnerPolicy.Instance,
        MemberCachePolicy.Voice  => VoicePolicy.Instance,
        MemberCachePolicy.Online => OnlinePolicy.Instance,
        _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unknown member cache policy preset.")
    };
}
