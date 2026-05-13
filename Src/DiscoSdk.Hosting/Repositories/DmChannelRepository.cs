using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Utils;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Repositories
{
    internal class DmChannelRepository(DiscordClient client)
    {
        private readonly SnowflakeCollection<IDmChannel> _channels = [];

        public IRestAction<IDmChannel> OpenDm(Snowflake userId)
        {
            return RestAction<IDmChannel>.Create(async cancellationToken =>
            {
                return await _channels.GetOrAddAsync(userId, async id =>
                {
                    var channelModel = await client.ChannelClient.CreateDMAsync(userId, cancellationToken);

                    return new DmChannelWrapper(client, channelModel);
                });
            });
        }

        public void SignalDmOpen(Channel channel)
        {

        }

        public bool Close(Snowflake userId)
        {
            return _channels.Remove(userId);
        }

        /// <summary>
        /// Evicts the cached DM channel whose id matches <paramref name="channelId"/>, regardless of which
        /// recipient it was opened for. Returns <c>false</c> if no such channel is cached.
        /// </summary>
        public bool CloseByChannelId(Snowflake channelId)
        {
            foreach (var (recipientId, channel) in _channels)
            {
                if (channel.Id == channelId)
                    return _channels.Remove(recipientId);
            }

            return false;
        }

        public IDmChannel[] GetAll()
        {
            return [.. _channels.Values];
        }
    }
}