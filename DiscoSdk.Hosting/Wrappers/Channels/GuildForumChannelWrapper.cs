using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildForumChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildForumChannelWrapper : GuildChannelWrapperBase, IGuildForumChannel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuildForumChannelWrapper"/> class.
    /// </summary>
    /// <param name="channel">The channel instance to wrap.</param>
    /// <param name="client">The Discord client for performing operations.</param>
    public GuildForumChannelWrapper(Channel channel, IGuild guild, DiscordClient client)
        : base(channel, guild, client)
    {
    }

    /// <inheritdoc />
    public DefaultReaction? DefaultReactionEmoji => _channel.DefaultReactionEmoji;

    /// <inheritdoc />
    public int? DefaultSortOrder => _channel.DefaultSortOrder;

    /// <inheritdoc />
    public int? DefaultForumLayout => _channel.DefaultForumLayout;

    /// <inheritdoc />
    public ForumLayout DefaultLayout => _channel.DefaultForumLayout.HasValue
        ? (ForumLayout)_channel.DefaultForumLayout.Value
        : ForumLayout.DefaultView;

    /// <inheritdoc />
    public int? DefaultThreadRateLimitPerUser => _channel.DefaultThreadRateLimitPerUser;

    /// <inheritdoc />
    public ForumTag[]? AvailableTags => _channel.AvailableTags;

    /// <inheritdoc />
    public ChannelFlags? Flags => _channel.Flags;

    // IThreadContainer implementation
    /// <inheritdoc />
    public int DefaultThreadSlowmode => _channel.DefaultThreadRateLimitPerUser ?? 0;

    /// <inheritdoc />
    public ICreateIThreadChannelAction CreateThreadChannel(string name, DiscordId messageId, bool isPrivate)
    {
        return new CreateThreadChannelAction(_client, this, name, messageId, isPrivate);
    }

    public IForumChannelManager GetManager()
    {
        return new ForumChannelManagerWrapper(Id, _client.ChannelClient);
    }

    /// <inheritdoc />
    public IThreadChannelPaginationAction GetThreadChannels()
    {
        return new ThreadChannelPaginationAction(_client, this, archived: false);
    }
}