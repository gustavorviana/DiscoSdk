using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Contexts;

internal class ChannelContext(DiscordClient client, IGuildChannelUnion channel) : GuildContextWrapper(client, channel.Guild), IChannelContext
{
    public IGuildChannelUnion Channel => channel;
}
