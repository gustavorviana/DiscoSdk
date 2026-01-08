using DiscoSdk.Models.Messages;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord channel that supports text-based communication.
/// </summary>
public interface ITextBasedChannel : IChannel
{
	/// <summary>
	/// Gets the ID of the last message sent in this channel.
	/// </summary>
	DiscordId? LastMessageId { get; }

	/// <summary>
	/// Gets the amount of seconds a user has to wait before sending another message.
	/// </summary>
	int? RateLimitPerUser { get; }

	/// <summary>
	/// Gets the topic of the channel.
	/// </summary>
	string? Topic { get; }

	/// <summary>
	/// Gets when the last pinned message was pinned.
	/// </summary>
	string? LastPinTimestamp { get; }

	/// <summary>
	/// Gets messages from this channel.
	/// </summary>
	/// <param name="limit">Maximum number of messages to return (1-100, default 50).</param>
	/// <param name="around">Get messages around this message ID.</param>
	/// <param name="before">Get messages before this message ID.</param>
	/// <param name="after">Get messages after this message ID.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of messages.</returns>
	Task<IMessage[]> GetMessagesAsync(int? limit = null, DiscordId? around = null, DiscordId? before = null, DiscordId? after = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a message by its ID from this channel.
	/// </summary>
	/// <param name="messageId">The ID of the message to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The message, or null if not found.</returns>
	Task<IMessage> GetMessageAsync(DiscordId messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a message from this channel.
	/// </summary>
	/// <param name="messageId">The ID of the message to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeleteMessageAsync(DiscordId messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes multiple messages from this channel in a single request.
	/// </summary>
	/// <param name="messageIds">The IDs of the messages to delete (2-100 messages).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task BulkDeleteMessagesAsync(DiscordId[] messageIds, CancellationToken cancellationToken = default);

	/// <summary>
	/// Triggers the typing indicator in this channel.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task TriggerTypingAsync(CancellationToken cancellationToken = default);
}

