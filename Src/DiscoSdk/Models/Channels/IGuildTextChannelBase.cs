using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a text channel in a Discord guild.
/// </summary>
public interface IGuildTextChannelBase : IGuildChannelBase, ITextBasedChannel
{
	/// <summary>
	/// Determines whether the specified member can talk in this channel.
	/// </summary>
	/// <param name="member">The member to check.</param>
	/// <returns>True if the member can talk, false otherwise.</returns>
	bool CanTalk(IMember member);

	/// <summary>
	/// Clears all reactions from a message by its ID.
	/// </summary>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeleteAllReactionsAsync(Snowflake messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Clears all reactions of a specific emoji from a message by its ID.
	/// </summary>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to clear reactions for.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task RemoveReactionAsync(Snowflake messageId, Emoji emoji, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a message with stickers to this channel.
	/// </summary>
	/// <param name="stickers">The collection of stickers to send.</param>
	/// <returns>A REST action that can be executed to send the message with stickers.</returns>
	ISendMessageRestAction SendStickers(IEnumerable<Snowflake> stickers);
}