using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord channel that supports text-based communication.
/// </summary>
public interface ITextBasedChannel : IChannel
{
	/// <summary>
	/// Gets the ID of the last message sent in this channel.
	/// </summary>
	Snowflake? LastMessageId { get; }

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
	DateTimeOffset? LastPinTimestamp { get; }

	/// <summary>
	/// Gets a message by its ID from this channel.
	/// </summary>
	/// <param name="messageId">The ID of the message to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The message, or null if not found.</returns>
	Task<IMessage?> GetMessageAsync(Snowflake messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a message from this channel.
	/// </summary>
	/// <param name="messageId">The ID of the message to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeleteMessageAsync(Snowflake messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes multiple messages from this channel in a single request.
	/// </summary>
	/// <param name="messageIds">The IDs of the messages to delete (2-100 messages).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task BulkDeleteMessagesAsync(Snowflake[] messageIds, CancellationToken cancellationToken = default);

	/// <summary>
	/// Purges (deletes) multiple messages by their IDs.
	/// </summary>
	/// <param name="messageIds">The IDs of the messages to purge.</param>
	/// <returns>An array of tasks representing the deletion operations.</returns>
	Task PurgeMessagesByIdAsync(params Snowflake[] messageIds);

	/// <summary>
	/// Purges (deletes) multiple messages.
	/// </summary>
	/// <param name="messages">The messages to purge.</param>
	/// <returns>An array of tasks representing the deletion operations.</returns>
	Task PurgeMessagesAsync(params IMessage[] messages);

	/// <summary>
	/// Purges (deletes) multiple messages.
	/// </summary>
	/// <param name="messages">The collection of messages to purge.</param>
	/// <returns>An array of tasks representing the deletion operations.</returns>
	Task PurgeMessagesAsync(IEnumerable<IMessage> messages);

	/// <summary>
	/// Sends a message to this channel.
	/// </summary>
	/// <returns>A REST action that can be executed to send the message.</returns>
	ISendMessageRestAction SendMessage();

	/// <summary>
	/// Triggers the typing indicator in this channel.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task TriggerTypingAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets an iterable message pagination action for this channel.
	/// </summary>
	/// <returns>A message pagination action.</returns>
	IMessagePaginationAction GetMessages();

	/// <summary>
	/// Adds a reaction to a message by its ID.
	/// </summary>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to react with.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task AddReactionByIdAsync(Snowflake messageId, Emoji emoji, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes a reaction from a message by its ID.
	/// </summary>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to remove the reaction for.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task RemoveReactionByIdAsync(Snowflake messageId, Emoji emoji, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves users who reacted with a specific emoji to a message.
	/// </summary>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to get reaction users for.</param>
	/// <returns>A pagination action for retrieving reaction users.</returns>
	IReactionPaginationAction RetrieveReactionUsersById(Snowflake messageId, Emoji emoji);

	/// <summary>
	/// Retrieves users who reacted with a specific emoji to a message.
	/// </summary>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to get reaction users for.</param>
	/// <param name="type">The type of reaction to retrieve.</param>
	/// <returns>A pagination action for retrieving reaction users.</returns>
	IReactionPaginationAction RetrieveReactionUsersById(Snowflake messageId, Emoji emoji, ReactionType type);

	/// <summary>
	/// Pins a message by its ID.
	/// </summary>
	/// <param name="messageId">The ID of the message to pin.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task PinMessageByIdAsync(Snowflake messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Unpins a message by its ID.
	/// </summary>
	/// <param name="messageId">The ID of the message to unpin.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task UnpinMessageByIdAsync(Snowflake messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves all pinned messages in this channel.
	/// </summary>
	/// <returns>A pagination action for retrieving pinned messages.</returns>
	IRestAction<IMessage[]> RetrievePinnedMessages();

	/// <summary>
	/// Edits a message by its ID.
	/// </summary>
	/// <param name="messageId">The ID of the message to edit.</param>
	/// <returns>A REST action that can be executed to edit the message.</returns>
	IEditMessageRestAction EditMessageById(Snowflake messageId);

	/// <summary>
	/// Ends a poll by its message ID.
	/// </summary>
	/// <param name="messageId">The ID of the message containing the poll.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The result contains the updated message.</returns>
	Task<IMessage> EndPollByIdAsync(Snowflake messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves poll voters by message ID and answer ID.
	/// </summary>
	/// <param name="messageId">The ID of the message containing the poll.</param>
	/// <param name="answerId">The ID of the poll answer.</param>
	/// <returns>A pagination action for retrieving poll voters.</returns>
	IPollVotersPaginationAction RetrievePollVotersById(Snowflake messageId, ulong answerId);
}