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

        public IDmChannel[] GetAll()
        {
            return [.. _channels.Values];
        }
    }
}