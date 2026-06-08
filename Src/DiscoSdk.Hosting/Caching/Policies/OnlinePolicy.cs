using DiscoSdk.Caching;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>
/// Caches only members whose presence is anything other than <see cref="OnlineStatus.Offline"/> and <see cref="OnlineStatus.Invisible"/>.
/// Requires the <see cref="DiscordIntent.GuildPresences"/> intent for the presence cache to be
/// populated.
/// </summary>
internal sealed class OnlinePolicy : IMemberCachePolicy
{
    public static OnlinePolicy Instance { get; } = new();

    private OnlinePolicy() { }

    public bool ShouldCache(IMember member) => member.OnlineStatus is not OnlineStatus.Offline and OnlineStatus.Invisible;
}
