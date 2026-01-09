using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildNewsChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GuildNewsChannelWrapper"/> class.
/// </remarks>
/// <param name="channel">The channel instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class GuildNewsChannelWrapper(Channel channel, IGuild guild, DiscordClient client)
    : GuildTextBasedChannelWrapper(channel, guild, client), IGuildNewsChannel
{
    public IRestAction<IMessage> CrosspostMessage(DiscordId messageId)
    {
        return RestAction<IMessage>.Create(async cancellationToken =>
        {
            var message = await _client.MessageClient.CrosspostAsync(_channel.Id, messageId, cancellationToken);
            return new MessageWrapper(this, message, _client, null);
        });
    }

    public IRestAction<FollowedChannel> FollowToChannel(DiscordId targetChannelId)
    {
        return RestAction<FollowedChannel>.Create(async cancellationToken =>
        {
            var path = $"channels/{Id}/followers";
            var request = new { webhook_channel_id = targetChannelId.ToString() };
            return await _client._client.SendJsonAsync<FollowedChannel>(path, HttpMethod.Post, request, cancellationToken);
        });
    }
}