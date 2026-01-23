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
    private readonly IGuild? _guild;
    private readonly DiscordClient _client;

    public EmojiWrapper(DiscordClient client, Emoji emoji, IGuild? guild)
    {
        _emoji = emoji ?? throw new ArgumentNullException(nameof(emoji));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _guild = guild ;

        if (_emoji.User != null)
            User = new UserWrapper(client, _emoji.User);
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

        if (_emoji.User is null || _guild is null)
            throw new InvalidOperationException("Cannot delete a non-guild emoji.");

        return RestAction.Create(async cancellationToken =>
        {
            await _client.GuildClient.DeleteEmojiAsync(
                _guild.Id,
                _emoji.Id.Value,
                cancellationToken
            );
        });
    }
}