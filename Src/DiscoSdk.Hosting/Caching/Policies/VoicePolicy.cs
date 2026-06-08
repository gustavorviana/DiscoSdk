using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>
/// Caches only members currently present in a voice channel. Relies on
/// <see cref="IMember.VoiceState"/> being populated, which requires the
/// <see cref="DiscordIntent.GuildVoiceStates"/> intent.
/// </summary>
internal sealed class VoicePolicy : IMemberCachePolicy
{
    public static VoicePolicy Instance { get; } = new();

    private VoicePolicy() { }

    public bool ShouldCache(IMember member) => member.VoiceState is not null;
}
