using DiscoSdk.Models;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for the Discord Soundboard Sound REST surface — listing, fetching, creating,
/// modifying and deleting guild-owned soundboard sounds.
/// </summary>
/// <remarks>
/// Discord wraps the list response in a single-field object (<see cref="SoundboardSoundListResponse"/>);
/// every other endpoint returns a bare <see cref="SoundboardSound"/>. Audio upload travels as a
/// JSON data URI in the <c>sound</c> field, not multipart.
/// </remarks>
internal class SoundboardSoundClient(IDiscordRestClient client)
{
    /// <summary>Lists every soundboard sound owned by a guild.</summary>
    public async Task<SoundboardSound[]> ListGuildSoundboardSoundsAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/soundboard-sounds", guildId);
        var envelope = await client.SendAsync<SoundboardSoundListResponse>(route, HttpMethod.Get, null, cancellationToken);
        return envelope.Items;
    }

    /// <summary>Gets a single guild-owned soundboard sound.</summary>
    public Task<SoundboardSound> GetGuildSoundboardSoundAsync(Snowflake guildId, Snowflake soundId, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/soundboard-sounds/{sound_id}", guildId, soundId);
        return client.SendAsync<SoundboardSound>(route, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Creates a guild soundboard sound. The <paramref name="body"/> object is serialized to JSON
    /// directly; callers build it via <c>CreateSoundboardSoundAction</c> to keep the partial-field
    /// (only-touched-keys) semantics consistent with the other builders.
    /// </summary>
    public Task<SoundboardSound> CreateGuildSoundboardSoundAsync(Snowflake guildId, object body, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(body);

        var route = new DiscordRoute("guilds/{guild_id}/soundboard-sounds", guildId);
        return client.SendAsync<SoundboardSound>(route, HttpMethod.Post, body, cancellationToken);
    }

    /// <summary>Modifies a guild soundboard sound's metadata (name / volume / emoji binding).</summary>
    public Task<SoundboardSound> ModifyGuildSoundboardSoundAsync(Snowflake guildId, Snowflake soundId, object body, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(body);

        var route = new DiscordRoute("guilds/{guild_id}/soundboard-sounds/{sound_id}", guildId, soundId);
        return client.SendAsync<SoundboardSound>(route, HttpMethod.Patch, body, cancellationToken);
    }

    /// <summary>Deletes a guild soundboard sound.</summary>
    public Task DeleteGuildSoundboardSoundAsync(Snowflake guildId, Snowflake soundId, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/soundboard-sounds/{sound_id}", guildId, soundId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
    }
}
