using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildSoundboardSurface(DiscordClient client, Snowflake guildId) : IGuildSoundboard
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public IRestAction<IReadOnlyList<ISoundboardSound>> GetAll()
        => RestAction<IReadOnlyList<ISoundboardSound>>.Create(async ct =>
        {
            var sounds = await _client.SoundboardSoundClient.ListGuildSoundboardSoundsAsync(guildId, ct).ConfigureAwait(false);
            return sounds.Select(s => (ISoundboardSound)new SoundboardSoundWrapper(_client, s)).ToList().AsReadOnly();
        });

    public IRestAction<ISoundboardSound> Get(Snowflake soundId)
        => RestAction<ISoundboardSound>.Create(async ct =>
            new SoundboardSoundWrapper(_client, await _client.SoundboardSoundClient.GetGuildSoundboardSoundAsync(guildId, soundId, ct).ConfigureAwait(false)));

    public ICreateSoundboardSoundAction Create(string name, DiscordSoundBuffer sound)
        => new CreateSoundboardSoundAction(_client, guildId, name, sound);
}
