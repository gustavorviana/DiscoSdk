using DiscoSdk.Contexts.Channels;
using DiscoSdk.Hosting.Contexts.Guilds;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ChannelContext(DiscordClient client, IGuildChannelUnion channel) : GuildContextWrapper(client, channel.Guild), IChannelContext
{
    public IGuildChannelUnion Channel => channel;

    IChannel IChannelContextBase.Channel => Channel;
}
