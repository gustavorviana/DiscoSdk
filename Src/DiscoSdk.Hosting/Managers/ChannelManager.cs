using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Utils;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Managers;

internal class ChannelManager(DiscordClient client)
{
    private readonly SnowflakeCollection<IChannel> _channels = [];

    public IRestAction<IChannel?> Get(Snowflake channelId, IGuild? guild)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        return RestAction<IChannel?>.Create(async cancellationToken =>
        {
            if (_channels.TryGetValue(channelId, out var channel))
                return channel;

            var channelModel = await client.ChannelClient.GetAsync(channelId, cancellationToken);
            if (channelModel == null)
                return null;

            channel = ChannelWrapper.ToSpecificType(client, channelModel, guild);
            _channels[channelId] = channel;
            return channel;
        });
    }

    public void OnChannelCreated(IChannel channel)
    {
        _channels.TryAdd(channel.Id, channel);
    }

    public void OnChannelRemoved(Snowflake channelId)
    {
        _channels.Remove(channelId);
    }

    public void OnChannelUpdated(Channel channel)
    {
        if (_channels.TryGetValue(channel.Id, out var value))
            (value as ChannelWrapper)?.OnUpdate(channel);
    }

    internal TextBasedChannelWrapper? GetWrappedTextChannel(Snowflake id)
    {
        if (_channels.TryGetValue(id, out var value))
            return value as TextBasedChannelWrapper;

        return null;
    }
}