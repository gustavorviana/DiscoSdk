using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper over the raw <see cref="SoundboardSound"/> POCO. Guild-owned sounds expose
/// Modify / Delete; Discord's built-in default sounds (returned by the global default-sounds
/// endpoint, not by this guild surface) refuse both with <see cref="InvalidOperationException"/>.
/// </summary>
internal sealed class SoundboardSoundWrapper(DiscordClient client, SoundboardSound model) : ISoundboardSound
{
    private const string ReadOnlyDefaultMessage =
        "Only guild-owned soundboard sounds can be modified. Discord's built-in default sounds are read-only.";

    /// <inheritdoc />
    public Snowflake Id => model.SoundId;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => model.SoundId.CreatedAt;

    /// <inheritdoc />
    public string Name => model.Name;

    /// <inheritdoc />
    public double Volume => model.Volume;

    /// <inheritdoc />
    public Snowflake? EmojiId => model.EmojiId is { } id && !id.Empty ? id : null;

    /// <inheritdoc />
    public string? EmojiName => model.EmojiName;

    /// <inheritdoc />
    public Snowflake? GuildId => model.GuildId.Empty ? null : model.GuildId;

    /// <inheritdoc />
    public bool Available => model.Available;

    /// <inheritdoc />
    public Snowflake? UserId => model.UserId.Empty ? null : model.UserId;

    /// <inheritdoc />
    public IModifySoundboardSoundAction Modify()
    {
        if (model.GuildId.Empty)
            throw new InvalidOperationException(ReadOnlyDefaultMessage);

        return new ModifySoundboardSoundAction(client, model.GuildId, model.SoundId);
    }

    /// <inheritdoc />
    public IRestAction Delete()
    {
        if (model.GuildId.Empty)
            throw new InvalidOperationException(ReadOnlyDefaultMessage);

        var guildId = model.GuildId;
        var soundId = model.SoundId;
        return RestAction.Create(ct => client.SoundboardSoundClient.DeleteGuildSoundboardSoundAsync(guildId, soundId, ct));
    }
}
