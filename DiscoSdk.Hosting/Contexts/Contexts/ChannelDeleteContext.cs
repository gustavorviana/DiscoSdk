using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Contexts;

internal class ChannelDeleteContext(DiscordClient client, IGuild guild, Snowflake channelId) : GuildContextWrapper(client, guild), IChannelDeleteContext
{
    public Snowflake ChannelId => channelId;
}
