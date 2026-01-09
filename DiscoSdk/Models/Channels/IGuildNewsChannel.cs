using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents an announcement channel (formerly news channel) in a Discord guild.
/// </summary>
public interface IGuildNewsChannel : IGuildChannelBase, ITextBasedChannel
{
    /// <summary>
    /// Crossposts a message in this announcement channel to following channels.
    /// </summary>
    /// <param name="messageId">The ID of the message to crosspost.</param>
    /// <returns>A REST action that can be executed to crosspost the message.</returns>
    IRestAction<IMessage> CrosspostMessage(DiscordId messageId);

    /// <summary>
    /// Follows this announcement channel to send messages to a target channel.
    /// </summary>
    /// <param name="targetChannelId">The ID of the target channel to follow to.</param>
    /// <returns>A REST action that can be executed to follow the channel.</returns>
    IRestAction<FollowedChannel> FollowToChannel(DiscordId targetChannelId);
}

