using DiscoSdk.Contexts.Channels;
using DiscoSdk.Hosting.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ChannelDeleteContext(DiscordClient client, IGuild guild, Snowflake channelId) : GuildContextWrapper(client, guild), IChannelDeleteContext
{
    public Snowflake ChannelId => channelId;
}
