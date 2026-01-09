using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers.Channels;

internal class GuildChannelUnionWrapper(Channel channel, IGuild guild, DiscordClient client) 
    : GuildChannelWrapperBase(channel, guild, client), IGuildChannelUnion
{
    public virtual IGuildTextChannel? AsTextChannel()
    {
        return _channel.Type == ChannelType.GuildText ? new GuildTextChannelWrapper(_channel, Guild, _client) : null;
    }

    public virtual IGuildNewsChannel? AsNewsChannel()
    {
        return _channel.Type == ChannelType.GuildAnnouncement ? new GuildNewsChannelWrapper(_channel, Guild, _client) : null;
    }

    public virtual IGuildThreadChannel? AsThreadChannel()
    {
        return _channel.Type is ChannelType.AnnouncementThread or ChannelType.PublicThread or ChannelType.PrivateThread
            ? new GuildThreadChannelWrapper(_channel, Guild, _client)
            : null;
    }

    public virtual IGuildVoiceChannel? AsVoiceChannel()
    {
        return _channel.Type == ChannelType.GuildVoice ? new GuildVoiceChannelWrapper(_channel, Guild, _client) : null;
    }

    public virtual IGuildStageChannel? AsStageChannel()
    {
        return _channel.Type == ChannelType.GuildStageVoice ? new GuildStageChannelWrapper(_channel, Guild, _client) : null;
    }

    public virtual IGuildForumChannel? AsForumChannel()
    {
        return _channel.Type == ChannelType.GuildForum ? new GuildForumChannelWrapper(_channel, Guild, _client) : null;
    }

    public virtual IGuildMediaChannel? AsMediaChannel()
    {
        return _channel.Type == ChannelType.GuildMedia ? new GuildMediaChannelWrapper(_channel, Guild, _client) : null;
    }
}