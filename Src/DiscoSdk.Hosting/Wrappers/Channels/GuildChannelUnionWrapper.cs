using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers.Channels;

internal class GuildChannelUnionWrapper(DiscordClient client, Channel channel, IGuild guild)
    : GuildChannelWrapperBase(client, channel, guild), IGuildChannelUnion
{
    public virtual IGuildTextChannel? AsTextChannel()
    {
        return _channel.Type == ChannelType.GuildText ? new GuildTextChannelWrapper(_client, _channel, Guild) : null;
    }

    public virtual IGuildNewsChannel? AsNewsChannel()
    {
        return _channel.Type == ChannelType.GuildAnnouncement ? new GuildNewsChannelWrapper(_client, _channel, Guild) : null;
    }

    public virtual IGuildThreadChannel? AsThreadChannel()
    {
        return _channel.Type is ChannelType.AnnouncementThread or ChannelType.PublicThread or ChannelType.PrivateThread
            ? new GuildThreadChannelWrapper(_client, _channel, Guild)
            : null;
    }

    public virtual IGuildVoiceChannel? AsVoiceChannel()
    {
        return _channel.Type == ChannelType.GuildVoice ? new GuildVoiceChannelWrapper(_client, _channel, Guild) : null;
    }

    public virtual IGuildStageChannel? AsStageChannel()
    {
        return _channel.Type == ChannelType.GuildStageVoice ? new GuildStageChannelWrapper(_client, _channel, Guild) : null;
    }

    public virtual IGuildForumChannel? AsForumChannel()
    {
        return _channel.Type == ChannelType.GuildForum ? new GuildForumChannelWrapper(_client, _channel, Guild) : null;
    }

    public virtual IGuildMediaChannel? AsMediaChannel()
    {
        return _channel.Type == ChannelType.GuildMedia ? new GuildMediaChannelWrapper(_channel, Guild, _client) : null;
    }

    public IChannel? ToExpectedChannel()
    {
        return channel.Type switch
        {
            ChannelType.GuildText => AsTextChannel(),
            ChannelType.GuildAnnouncement => AsNewsChannel(),
            ChannelType.AnnouncementThread or ChannelType.PublicThread or ChannelType.PrivateThread => AsThreadChannel(),
            ChannelType.GuildVoice => AsVoiceChannel(),
            ChannelType.GuildStageVoice => AsStageChannel(),
            ChannelType.GuildForum => AsForumChannel(),
            ChannelType.GuildMedia => AsMediaChannel(),
            _ => null
        };
    }
}