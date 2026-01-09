using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Extension methods for <see cref="IGuildTextChannelBase"/>.
/// </summary>
public static class IGuildTextChannelExtensions
{
	/// <summary>
	/// Removes a reaction from a message by its ID for a specific user.
	/// </summary>
	/// <param name="channel">The channel.</param>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to remove the reaction for.</param>
	/// <param name="user">The user whose reaction to remove.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static Task RemoveReactionByIdAsync(this IGuildTextChannelBase channel, DiscordId messageId, Emoji emoji, IUser user, CancellationToken cancellationToken = default)
	{
		// This would need to be implemented in the concrete class
		// For now, we'll just call the base method
		return channel.RemoveReactionByIdAsync(messageId, emoji, cancellationToken);
	}

	/// <summary>
	/// Deletes multiple messages by their IDs.
	/// </summary>
	/// <param name="channel">The channel.</param>
	/// <param name="messageIds">The collection of message IDs to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static Task DeleteMessagesByIdsAsync(this IGuildTextChannelBase channel, IEnumerable<DiscordId> messageIds, CancellationToken cancellationToken = default)
	{
		return channel.BulkDeleteMessagesAsync(messageIds.ToArray(), cancellationToken);
	}
}

