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
internal class GuildMediaChannelWrapper(Channel channel, IGuild guild, DiscordClient client) : GuildChannelWrapperBase(channel, guild, client), IGuildMediaChannel
{
    public DefaultReaction? DefaultReactionEmoji => _channel.DefaultReactionEmoji;
    public int? DefaultSortOrder => _channel.DefaultSortOrder;
    public int? DefaultForumLayout => _channel.DefaultForumLayout;
    public int? DefaultThreadRateLimitPerUser => _channel.DefaultThreadRateLimitPerUser;
    public ForumTag[]? AvailableTags => _channel.AvailableTags;
    public ChannelFlags? Flags => _channel.Flags;
    public int DefaultThreadSlowmode => _channel.DefaultThreadRateLimitPerUser ?? 0;

    public ICreateIThreadChannelAction CreateThreadChannel(string name, Snowflake messageId, bool isPrivate)
    {
        return new CreateThreadChannelAction(_client, this, name, messageId, isPrivate);
    }

    public IThreadChannelPaginationAction GetThreadChannels()
    {
        return new ThreadChannelPaginationAction(_client, this, archived: false);
    }
}