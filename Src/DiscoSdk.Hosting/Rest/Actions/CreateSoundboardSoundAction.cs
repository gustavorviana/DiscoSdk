using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class CreateSoundboardSoundAction : RestAction<ISoundboardSound>, ICreateSoundboardSoundAction
{
    private readonly DiscordClient _client;
    private readonly Snowflake _guildId;
    private string _name;
    private DiscordSoundBuffer _sound;
    private double? _volume;
    private Snowflake? _emojiId;
    private string? _emojiName;

    public CreateSoundboardSoundAction(DiscordClient client, Snowflake guildId, string name, DiscordSoundBuffer sound)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _guildId = guildId;
        _name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        _sound = sound ?? throw new ArgumentNullException(nameof(sound));
    }

    /// <inheritdoc />
    public ICreateSoundboardSoundAction SetName(string name)
    {
        _name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        return this;
    }

    /// <inheritdoc />
    public ICreateSoundboardSoundAction SetSound(DiscordSoundBuffer sound)
    {
        _sound = sound ?? throw new ArgumentNullException(nameof(sound));
        return this;
    }

    /// <inheritdoc />
    public ICreateSoundboardSoundAction SetVolume(double volume)
    {
        if (volume < 0d || volume > 1d)
            throw new ArgumentOutOfRangeException(nameof(volume), volume, "Volume must lie in the closed range [0, 1].");
        _volume = volume;
        return this;
    }

    /// <inheritdoc />
    public ICreateSoundboardSoundAction SetEmoji(Snowflake emojiId)
    {
        _emojiId = emojiId;
        _emojiName = null;
        return this;
    }

    /// <inheritdoc />
    public ICreateSoundboardSoundAction SetEmoji(string unicodeEmoji)
    {
        if (string.IsNullOrWhiteSpace(unicodeEmoji))
            throw new ArgumentException("Unicode emoji cannot be null or empty.", nameof(unicodeEmoji));
        _emojiName = unicodeEmoji;
        _emojiId = null;
        return this;
    }

    /// <inheritdoc />
    public override async Task<ISoundboardSound> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["name"] = _name,
            ["sound"] = _sound.ToDataUri(),
        };

        if (_volume is { } v)
            body["volume"] = v;

        if (_emojiId is { } eid)
            body["emoji_id"] = eid.ToString();
        else if (_emojiName is { } name)
            body["emoji_name"] = name;

        var model = await _client.SoundboardSoundClient.CreateGuildSoundboardSoundAsync(_guildId, body, cancellationToken);
        return new SoundboardSoundWrapper(_client, model);
    }
}
