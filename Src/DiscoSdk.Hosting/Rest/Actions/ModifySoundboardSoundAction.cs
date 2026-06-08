using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifySoundboardSoundAction(DiscordClient client, Snowflake guildId, Snowflake soundId)
    : RestAction<ISoundboardSound>, IModifySoundboardSoundAction
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly Snowflake _guildId = guildId;
    private readonly Snowflake _soundId = soundId;

    private string? _name;
    private double? _volume;
    // Tri-state for emoji: unset, set to id, set to name, or explicitly cleared (both null).
    private EmojiState _emojiState;
    private Snowflake? _emojiId;
    private string? _emojiName;

    /// <inheritdoc />
    public IModifySoundboardSoundAction SetName(string name)
    {
        _name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        return this;
    }

    /// <inheritdoc />
    public IModifySoundboardSoundAction SetVolume(double volume)
    {
        if (volume < 0d || volume > 1d)
            throw new ArgumentOutOfRangeException(nameof(volume), volume, "Volume must lie in the closed range [0, 1].");
        _volume = volume;
        return this;
    }

    /// <inheritdoc />
    public IModifySoundboardSoundAction SetEmoji(Snowflake emojiId)
    {
        _emojiId = emojiId;
        _emojiName = null;
        _emojiState = EmojiState.SetId;
        return this;
    }

    /// <inheritdoc />
    public IModifySoundboardSoundAction SetEmoji(string unicodeEmoji)
    {
        if (string.IsNullOrWhiteSpace(unicodeEmoji))
            throw new ArgumentException("Unicode emoji cannot be null or empty.", nameof(unicodeEmoji));
        _emojiName = unicodeEmoji;
        _emojiId = null;
        _emojiState = EmojiState.SetName;
        return this;
    }

    /// <inheritdoc />
    public IModifySoundboardSoundAction ClearEmoji()
    {
        _emojiId = null;
        _emojiName = null;
        _emojiState = EmojiState.Cleared;
        return this;
    }

    /// <inheritdoc />
    public override async Task<ISoundboardSound> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();

        if (_name is { } name)
            body["name"] = name;

        if (_volume is { } v)
            body["volume"] = v;

        switch (_emojiState)
        {
            case EmojiState.SetId:
                body["emoji_id"] = _emojiId!.Value.ToString();
                body["emoji_name"] = null;
                break;
            case EmojiState.SetName:
                body["emoji_name"] = _emojiName;
                body["emoji_id"] = null;
                break;
            case EmojiState.Cleared:
                body["emoji_id"] = null;
                body["emoji_name"] = null;
                break;
            case EmojiState.Untouched:
                break;
        }

        var model = await _client.SoundboardSoundClient.ModifyGuildSoundboardSoundAsync(_guildId, _soundId, body, cancellationToken);
        return new SoundboardSoundWrapper(_client, model);
    }

    private enum EmojiState
    {
        Untouched,
        SetId,
        SetName,
        Cleared,
    }
}
