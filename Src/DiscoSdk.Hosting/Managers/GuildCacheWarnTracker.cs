using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Managers;

/// <summary>
/// Per-process gate that emits a single Warning when the guild cache is read while the
/// <see cref="DiscordIntent.Guilds"/> intent is not enabled. Without the intent Discord never
/// dispatches <c>GUILD_CREATE</c>, so the cache stays empty forever and consumers see "I have
/// zero guilds" silently. The diagnostic surfaces the misconfig the first time any cache
/// accessor returns the empty set.
/// </summary>
internal static class GuildCacheWarnTracker
{
    private static int _warned;

    public static void WarnMissing(ILogger logger)
    {
        if (Interlocked.CompareExchange(ref _warned, 1, 0) != 0)
            return;

        logger.LogWarning(
            "Read the guild cache without the Guilds intent enabled — Discord never sends GUILD_CREATE without it, so the cache will stay empty and every cached lookup returns null. Add DiscordIntent.Guilds on DiscordClientBuilder.WithIntents. This warning logs once per process.");
    }

    internal static void ResetForTests() => Interlocked.Exchange(ref _warned, 0);
}
