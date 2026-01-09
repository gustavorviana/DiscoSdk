using DiscoSdk.Exceptions;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a Discord message with visual properties and operations.
/// </summary>
public interface IMessage : IDeletable, IMentionable
{
	/// <summary>
	/// Gets the channel the message was sent in.
	/// </summary>
	ITextBasedChannel Channel { get; }

	/// <summary>
	/// Gets the ID of the guild the message was sent in.
	/// </summary>
	IGuild? Guild { get; }

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
	IReaction[] Reactions { get; }

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
	MessageFlags Flags { get; }

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

	/// <summary>
	/// Crossposts this message (only for news channels).
	/// </summary>
	/// <returns>A REST action that can be executed to crosspost the message.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction<IMessage> Crosspost();

	/// <summary>
	/// Adds a reaction to this message.
	/// </summary>
	/// <param name="emoji">The emoji to react with (URL-encoded if custom emoji).</param>
	/// <returns>A REST action that can be executed to add the reaction.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction AddReaction(string emoji);

	/// <summary>
	/// Gets all users who reacted with a specific emoji to this message.
	/// </summary>
	/// <param name="emoji">The emoji to get reactions for (URL-encoded if custom emoji).</param>
	/// <param name="after">Get users after this user ID.</param>
	/// <param name="limit">Maximum number of users to return (1-100, default 25).</param>
	/// <returns>A REST action that can be executed to get the reactions.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction<User[]> GetReactions(string emoji, string? after = null, int? limit = null);

	/// <summary>
	/// Deletes all reactions of a specific emoji from this message.
	/// </summary>
	/// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
	/// <returns>A REST action that can be executed to delete all reactions for the emoji.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction DeleteAllReactionsForEmoji(string emoji);

	/// <summary>
	/// Deletes all reactions from this message.
	/// </summary>
	/// <returns>A REST action that can be executed to delete all reactions.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction DeleteAllReactions();

	/// <summary>
	/// Pins this message in the channel.
	/// </summary>
	/// <returns>A REST action that can be executed to pin the message.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	/// <exception cref="EphemeralMessageException">Thrown when attempting to pin an ephemeral message.</exception>
	IRestAction Pin();

	/// <summary>
	/// Unpins this message from the channel.
	/// </summary>
	/// <returns>A REST action that can be executed to unpin the message.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	/// <exception cref="EphemeralMessageException">Thrown when attempting to unpin an ephemeral message.</exception>
	IRestAction Unpin();
}