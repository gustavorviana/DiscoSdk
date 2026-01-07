using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a Discord message with visual properties and operations.
/// </summary>
public interface IMessage
{
	// Visual Properties
	/// <summary>
	/// Gets the ID of the message.
	/// </summary>
	DiscordId Id { get; }

	/// <summary>
	/// Gets the ID of the channel the message was sent in.
	/// </summary>
	DiscordId ChannelId { get; }

	/// <summary>
	/// Gets the ID of the guild the message was sent in.
	/// </summary>
	DiscordId? GuildId { get; }

	/// <summary>
	/// Gets the author of the message.
	/// </summary>
	User Author { get; }

	/// <summary>
	/// Gets the contents of the message.
	/// </summary>
	string Content { get; }

	/// <summary>
	/// Gets any embedded content.
	/// </summary>
	Embed[] Embeds { get; }

	/// <summary>
	/// Gets the components of the message.
	/// </summary>
	MessageComponent[]? Components { get; }

	/// <summary>
	/// Gets any attached files.
	/// </summary>
	Attachment[] Attachments { get; }

	/// <summary>
	/// Gets the reactions to the message.
	/// </summary>
	Reaction[]? Reactions { get; }

	/// <summary>
	/// Gets when this message was sent.
	/// </summary>
	string Timestamp { get; }

	/// <summary>
	/// Gets when this message was edited.
	/// </summary>
	string? EditedTimestamp { get; }

	/// <summary>
	/// Gets a value indicating whether this message is pinned.
	/// </summary>
	bool Pinned { get; }

	/// <summary>
	/// Gets the message flags.
	/// </summary>
	MessageFlags? Flags { get; }

	/// <summary>
	/// Gets the type of message.
	/// </summary>
	MessageType Type { get; }

	// Operations with Builders

	/// <summary>
	/// Creates a builder for editing this message.
	/// </summary>
	/// <returns>An <see cref="IEditMessageRestAction"/> instance for editing the message.</returns>
	IEditMessageRestAction Edit();

	/// <summary>
	/// Creates a builder for replying to this message.
	/// </summary>
	/// <param name="content">The initial reply content.</param>
	/// <returns>An <see cref="ISendMessageRestAction"/> instance for sending the reply.</returns>
	ISendMessageRestAction Reply(string? content = null);

	// Direct Operations

	/// <summary>
	/// Deletes this message.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeleteAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	void Delete();

	/// <summary>
	/// Crossposts this message (only for news channels).
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The crossposted message.</returns>
	Task<IMessage> CrosspostAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Crossposts this message synchronously (only for news channels).
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <returns>The crossposted message.</returns>
	IMessage Crosspost();

	// Reaction Operations

	/// <summary>
	/// Adds a reaction to this message.
	/// </summary>
	/// <param name="emoji">The emoji to react with (URL-encoded if custom emoji).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task AddReactionAsync(string emoji, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a reaction to this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="emoji">The emoji to react with (URL-encoded if custom emoji).</param>
	void AddReaction(string emoji);

	/// <summary>
	/// Removes a reaction from this message (current user's reaction).
	/// </summary>
	/// <param name="emoji">The emoji to remove (URL-encoded if custom emoji).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task RemoveReactionAsync(string emoji, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes a reaction from this message synchronously (current user's reaction).
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="emoji">The emoji to remove (URL-encoded if custom emoji).</param>
	void RemoveReaction(string emoji);

	/// <summary>
	/// Removes a reaction from this message for a specific user.
	/// </summary>
	/// <param name="emoji">The emoji to remove (URL-encoded if custom emoji).</param>
	/// <param name="userId">The ID of the user whose reaction to remove. Use "@me" for the current user.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task RemoveUserReactionAsync(string emoji, string userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes a reaction from this message for a specific user synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="emoji">The emoji to remove (URL-encoded if custom emoji).</param>
	/// <param name="userId">The ID of the user whose reaction to remove. Use "@me" for the current user.</param>
	void RemoveUserReaction(string emoji, string userId);

	/// <summary>
	/// Gets all users who reacted with a specific emoji to this message.
	/// </summary>
	/// <param name="emoji">The emoji to get reactions for (URL-encoded if custom emoji).</param>
	/// <param name="after">Get users after this user ID.</param>
	/// <param name="limit">Maximum number of users to return (1-100, default 25).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of users who reacted.</returns>
	Task<User[]> GetReactionsAsync(string emoji, string? after = null, int? limit = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all users who reacted with a specific emoji to this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="emoji">The emoji to get reactions for (URL-encoded if custom emoji).</param>
	/// <param name="after">Get users after this user ID.</param>
	/// <param name="limit">Maximum number of users to return (1-100, default 25).</param>
	/// <returns>An array of users who reacted.</returns>
	User[] GetReactions(string emoji, string? after = null, int? limit = null);

	/// <summary>
	/// Deletes all reactions of a specific emoji from this message.
	/// </summary>
	/// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeleteAllReactionsForEmojiAsync(string emoji, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes all reactions of a specific emoji from this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
	void DeleteAllReactionsForEmoji(string emoji);

	/// <summary>
	/// Deletes all reactions from this message.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeleteAllReactionsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes all reactions from this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	void DeleteAllReactions();

	/// <summary>
	/// Gets the guild this message was sent in.
	/// </summary>
	/// <returns>The guild this message was sent in, or null if the message was sent in a DM.</returns>
	/// <exception cref="NotSupportedException">This operation is not yet supported.</exception>
	Guild? GetGuild();
}

