using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Channels;

public interface IChannelDeleteContext : IGuildContext
{
    Snowflake ChannelId { get; }
}
