using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildMediaChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GuildMediaChannelWrapper"/> class.
/// </remarks>
/// <param name="channel">The channel instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class GuildMediaChannelWrapper(Channel channel, IGuild guild, DiscordClient client) : GuildChannelWrapperBase(client, channel, guild), IGuildMediaChannel
{
    public DefaultReaction? DefaultReactionEmoji => _channel.DefaultReactionEmoji;
    public SortOrderType? DefaultSortOrder => _channel.DefaultSortOrder is { } sortOrder ? (SortOrderType)sortOrder : null;
    public ForumTag[]? AvailableTags => _channel.AvailableTags;
    public ChannelFlags? Flags => _channel.Flags;
    public int DefaultThreadSlowmode => _channel.DefaultThreadRateLimitPerUser ?? 0;

    public ICreateIThreadChannelAction CreateThreadChannel(string name, Snowflake messageId, bool isPrivate)
    {
        return new CreateThreadChannelAction(_client, this, name, messageId, isPrivate);
    }

    public ICreateIThreadChannelAction StartPost(string name)
    {
        return new CreateThreadChannelAction(_client, this, name);
    }

    public IThreadChannelPaginationAction GetThreadChannels()
    {
        return new ThreadChannelPaginationAction(_client, this, archived: false);
    }
}