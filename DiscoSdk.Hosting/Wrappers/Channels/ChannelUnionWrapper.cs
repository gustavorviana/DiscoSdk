using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers.Channels;

internal class ChannelUnionWrapper(Channel channel, IGuild guild, DiscordClient client) 
    : ChannelWrapper(channel, client), IChannelUnion
{
    public IDmChannel? AsDmChannel()
    {
        return _channel.Type is ChannelType.Dm or ChannelType.GroupDm 
            ? new DmChannelWrapper(_channel, _client) 
            : null;
    }

    public virtual IGuildTextChannel? AsTextChannel()
    {
        return _channel.Type == ChannelType.GuildText ? new GuildTextChannelWrapper(_channel, guild, _client) : null;
    }

    public virtual IGuildNewsChannel? AsNewsChannel()
    {
        return _channel.Type == ChannelType.GuildAnnouncement ? new GuildNewsChannelWrapper(_channel, guild, _client) : null;
    }

    public virtual IGuildThreadChannel? AsThreadChannel()
    {
        return _channel.Type is ChannelType.AnnouncementThread or ChannelType.PublicThread or ChannelType.PrivateThread
            ? new GuildThreadChannelWrapper(_channel, guild, _client)
            : null;
    }

    public virtual IGuildVoiceChannel? AsVoiceChannel()
    {
        return _channel.Type == ChannelType.GuildVoice ? this as IGuildVoiceChannel : null;
    }

    public virtual IGuildStageChannel? AsStageChannel()
    {
        return _channel.Type == ChannelType.GuildStageVoice ? this as IGuildStageChannel : null;
    }

    public virtual IGuildCategory? AsCategory()
    {
        return _channel.Type == ChannelType.GuildCategory ? this as IGuildCategory : null;
    }

    public virtual IGuildForumChannel? AsForumChannel()
    {
        return _channel.Type == ChannelType.GuildForum ? new GuildForumChannelWrapper(_channel, guild, _client) : null;
    }

    public virtual IGuildMediaChannel? AsMediaChannel()
    {
        return _channel.Type == ChannelType.GuildMedia ? new GuildMediaChannelWrapper(_channel, guild, _client) : null;
    }

    public virtual IGuildMessageChannel? AsGuildMessageChannel()
    {
        return this as IGuildMessageChannel;
    }

    public virtual IGuildAudioChannel? AsAudioChannel()
    {
        return this as IGuildAudioChannel;
    }

    public virtual IThreadContainer? AsThreadContainer()
    {
        return this as IThreadContainer;
    }

    public virtual IStandardGuildChannel? AsStandardGuildChannel()
    {
        return _channel.Type != ChannelType.GuildCategory ? this as IStandardGuildChannel : null;
    }

    public virtual IStandardGuildMessageChannel? AsStandardGuildMessageChannel()
    {
        return _channel.Type is ChannelType.GuildText or ChannelType.GuildAnnouncement
            ? this as IStandardGuildMessageChannel
            : null;
    }
}