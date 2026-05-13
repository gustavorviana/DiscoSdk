using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildTextChannel"/> for a <see cref="Channel"/> instance.
/// All text-channel behaviour lives in <see cref="GuildTextBasedChannelWrapper"/>; this type only
/// adds the text-channel manager.
/// </summary>
internal class GuildTextChannelWrapper : GuildTextBasedChannelWrapper, IGuildTextChannel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuildTextChannelWrapper"/> class.
    /// </summary>
    /// <param name="channel">The channel instance to wrap.</param>
    /// <param name="guild">The guild this channel belongs to.</param>
    /// <param name="client">The Discord client for performing operations.</param>
    public GuildTextChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
        : base(client, channel, guild)
    {
    }

    public ITextChannelManager GetManager()
    {
        return new TextChannelManagerWrapper(Id, _client.ChannelClient);
    }
}
