using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IEmoji"/> for an <see cref="Emoji"/> instance.
/// </summary>
internal class EmojiWrapper : IEmoji
{
    private readonly Emoji _emoji;
    private readonly IGuild _guild;
    private readonly DiscordClient _client;

    public EmojiWrapper(Emoji emoji, IGuild guild, DiscordClient client)
    {
        _emoji = emoji ?? throw new ArgumentNullException(nameof(emoji));
        _guild = guild ?? throw new ArgumentNullException(nameof(guild));
        _client = client ?? throw new ArgumentNullException(nameof(client));

        if (_emoji.User != null)
            User = new UserWrapper(_emoji.User, client);
    }

    /// <inheritdoc />
    public Snowflake Id => _emoji.Id ?? throw new InvalidOperationException("Emoji must have an ID.");

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => _emoji.Id?.CreatedAt ?? throw new InvalidOperationException("Emoji must have an ID.");

    /// <inheritdoc />
    public string? Name => _emoji.Name;

    /// <inheritdoc />
    public string[] Roles => _emoji.Roles;

    /// <inheritdoc />
    public IUser? User { get; }

    /// <inheritdoc />
    public bool RequireColons => _emoji.RequireColons;

    /// <inheritdoc />
    public bool IsManaged => _emoji.Managed;

    /// <inheritdoc />
    public bool IsAnimated => _emoji.Animated;

    /// <inheritdoc />
    public bool Available => _emoji.Available;

    /// <inheritdoc />
    public IGuild? Guild => _guild;

    /// <inheritdoc />
    public IEditEmojiAction Edit()
    {
        if (!_emoji.Id.HasValue)
            throw new InvalidOperationException("Cannot edit an emoji without an ID.");

        return new EditEmojiAction(_client, _guild, _emoji.Id.Value);
    }

    /// <inheritdoc />
    public IRestAction Delete()
    {
        if (!_emoji.Id.HasValue)
            throw new InvalidOperationException("Cannot delete an emoji without an ID.");

        return RestAction.Create(async cancellationToken =>
        {
            await _client.GuildClient.DeleteEmojiAsync(_guild.Id, _emoji.Id.Value, cancellationToken);
        });
    }
}