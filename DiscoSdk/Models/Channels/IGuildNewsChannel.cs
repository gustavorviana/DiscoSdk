using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents an announcement channel (formerly news channel) in a Discord guild.
/// </summary>
public interface IGuildNewsChannel : IGuildChannel, IGuildMessageChannel
{
	/// <summary>
	/// Gets a manager to edit this news channel.
	/// </summary>
	/// <returns>A manager that can be configured and executed to edit the channel.</returns>
	/// <remarks>
	/// The manager is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	INewsChannelManager GetManager();

	/// <summary>
	/// Crossposts a message in this announcement channel to following channels.
	/// </summary>
	/// <param name="messageId">The ID of the message to crosspost.</param>
	/// <returns>A REST action that can be executed to crosspost the message.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction<IMessage> CrosspostMessage(DiscordId messageId);

	/// <summary>
	/// Follows this announcement channel to send messages to a target channel.
	/// </summary>
	/// <param name="targetChannelId">The ID of the target channel to follow to.</param>
	/// <returns>A REST action that can be executed to follow the channel.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction<FollowedChannel> Follow(DiscordId targetChannelId);

	/// <summary>
	/// Follows this announcement channel to send messages to a target channel.
	/// </summary>
	/// <param name="targetChannel">The target channel to follow to.</param>
	/// <returns>A REST action that can be executed to follow the channel.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// Requires MANAGE_WEBHOOKS permission in the target channel.
	/// </remarks>
	IRestAction<FollowedChannel> Follow(IGuildTextChannel targetChannel);
}

