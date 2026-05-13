using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGroupDmChannel"/> for a group direct-message <see cref="Channel"/>.
/// </summary>
internal class GroupDmChannelWrapper(DiscordClient client, Channel channel)
    : DmChannelWrapper(client, channel), IGroupDmChannel
{
    /// <inheritdoc />
    public Snowflake OwnerId => _channel.OwnerId ?? default;
}
