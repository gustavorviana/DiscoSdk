using DiscoSdk.Models;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Extension methods for <see cref="ITextBasedChannel"/>.
/// </summary>
public static class TextChannelExtensions
{
	/// <summary>
	/// Gets a message by its ID from this channel synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="channel">The channel to get the message from.</param>
	/// <param name="messageId">The ID of the message to retrieve.</param>
	/// <returns>The message, or null if not found.</returns>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="ITextBasedChannel.GetMessageAsync"/> when possible.
	/// </remarks>
	public static IMessage? GetMessage(this ITextBasedChannel channel, DiscordId messageId)
	{
		return channel.GetMessageAsync(messageId).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Gets messages from this channel synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="channel">The channel to get messages from.</param>
	/// <param name="limit">Maximum number of messages to return (1-100, default 50).</param>
	/// <param name="around">Get messages around this message ID.</param>
	/// <param name="before">Get messages before this message ID.</param>
	/// <param name="after">Get messages after this message ID.</param>
	/// <returns>An array of messages.</returns>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="ITextBasedChannel.GetMessagesAsync"/> when possible.
	/// </remarks>
	public static IMessage[] GetMessages(this ITextBasedChannel channel, int? limit = null, DiscordId? around = null, DiscordId? before = null, DiscordId? after = null)
	{
		return channel.GetMessagesAsync(limit, around, before, after).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Deletes a message from this channel synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="channel">The channel to delete the message from.</param>
	/// <param name="messageId">The ID of the message to delete.</param>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="ITextBasedChannel.DeleteMessageAsync"/> when possible.
	/// </remarks>
	public static void DeleteMessage(this ITextBasedChannel channel, DiscordId messageId)
	{
		channel.DeleteMessageAsync(messageId).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Deletes multiple messages from this channel synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="channel">The channel to delete messages from.</param>
	/// <param name="messageIds">The IDs of the messages to delete (2-100 messages).</param>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="ITextBasedChannel.BulkDeleteMessagesAsync"/> when possible.
	/// </remarks>
	public static void BulkDeleteMessages(this ITextBasedChannel channel, DiscordId[] messageIds)
	{
		channel.BulkDeleteMessagesAsync(messageIds).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Triggers the typing indicator in this channel synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="channel">The channel to trigger typing in.</param>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="ITextBasedChannel.TriggerTypingAsync"/> when possible.
	/// </remarks>
	public static void TriggerTyping(this ITextBasedChannel channel)
	{
		channel.TriggerTypingAsync().GetAwaiter().GetResult();
	}
}

