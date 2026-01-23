using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
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
internal class GuildNewsChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
    : GuildTextBasedChannelWrapper(client, channel, guild), IGuildNewsChannel
{
    public IRestAction<IMessage> CrosspostMessage(Snowflake messageId)
    {
        return RestAction<IMessage>.Create(async cancellationToken =>
        {
            var message = await _client.MessageClient.CrosspostAsync(_channel.Id, messageId, cancellationToken);
            return new MessageWrapper(_client, this, message, null);
        });
    }

    public IRestAction<FollowedChannel> Follow(Snowflake targetChannelId)
    {
        return RestAction<FollowedChannel>.Create(async cancellationToken =>
        {
            return await _client.ChannelClient.FollowAsync(Id, targetChannelId, cancellationToken);
        });
    }

    public IRestAction<FollowedChannel> Follow(IGuildTextChannel targetChannel)
    {
        ArgumentNullException.ThrowIfNull(targetChannel);

        return RestAction<FollowedChannel>.Create(async cancellationToken =>
        {
            var selfMember = await Guild.GetMember(Snowflake.Parse(_client.BotUser.Id)).ExecuteAsync(cancellationToken)
            ?? throw new InvalidOperationException("Cannot get self member from guild.");

            var permission = targetChannel.GetPermission(selfMember);
            if (!permission.HasFlag(DiscordPermission.ViewChannel))
                throw new InsufficientPermissionException("Cannot access target channel.", "VIEW_CHANNEL");

            if (!permission.HasFlag(DiscordPermission.ManageWebhooks))
                throw InsufficientPermissionException.Operation("MANAGE_WEBHOOKS", "follow announcement channel");

            return await Follow(targetChannel.Id).ExecuteAsync(cancellationToken);
        });
    }

    public INewsChannelManager GetManager()
    {
        return new NewsChannelManagerWrapper(Id, _client.ChannelClient);
    }
}