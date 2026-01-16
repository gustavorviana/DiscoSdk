using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IDmChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DmChannelWrapper"/> class.
/// </remarks>
/// <param name="channel">The channel instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class DmChannelWrapper(Channel channel, DiscordClient client) : TextBasedChannelWrapper(channel, client), IDmChannel
{
    /// <inheritdoc />
    public Snowflake OwnerId => _channel.OwnerId ?? default;

    public override string Name => _channel.Recipients?.FirstOrDefault()?.GlobalName ?? base.Name;

    public override IRestAction Delete()
    {
        return RestAction.Create(async cancellationToken =>
        {
            await base.Delete().ExecuteAsync(cancellationToken);
            _client.DmRepository.Close(OwnerId);
        });
    }
}