namespace DiscoSdk.Models.Messages;

/// <summary>
/// Extension methods for <see cref="IMessage"/>.
/// </summary>
public static class MessageExtensions
{
	/// <summary>
	/// Crossposts this message synchronously (only for news channels).
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="message">The message to crosspost.</param>
	/// <returns>The crossposted message.</returns>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="IMessage.CrosspostAsync"/> when possible.
	/// </remarks>
	public static IMessage Crosspost(this IMessage message)
	{
		return message.CrosspostAsync().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Adds a reaction to this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="message">The message to add a reaction to.</param>
	/// <param name="emoji">The emoji to react with (URL-encoded if custom emoji).</param>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="IMessage.AddReactionAsync"/> when possible.
	/// </remarks>
	public static void AddReaction(this IMessage message, string emoji)
	{
		message.AddReactionAsync(emoji).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Gets all users who reacted with a specific emoji to this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="message">The message to get reactions from.</param>
	/// <param name="emoji">The emoji to get reactions for (URL-encoded if custom emoji).</param>
	/// <param name="after">Get users after this user ID.</param>
	/// <param name="limit">Maximum number of users to return (1-100, default 25).</param>
	/// <returns>An array of users who reacted.</returns>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="IMessage.GetReactionsAsync"/> when possible.
	/// </remarks>
	public static User[] GetReactions(this IMessage message, string emoji, string? after = null, int? limit = null)
	{
		return message.GetReactionsAsync(emoji, after, limit).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Deletes all reactions of a specific emoji from this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="message">The message to delete reactions from.</param>
	/// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="IMessage.DeleteAllReactionsForEmojiAsync"/> when possible.
	/// </remarks>
	public static void DeleteAllReactionsForEmoji(this IMessage message, string emoji)
	{
		message.DeleteAllReactionsForEmojiAsync(emoji).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Deletes all reactions from this message synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="message">The message to delete all reactions from.</param>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="IMessage.DeleteAllReactionsAsync"/> when possible.
	/// </remarks>
	public static void DeleteAllReactions(this IMessage message)
	{
		message.DeleteAllReactionsAsync().GetAwaiter().GetResult();
	}
}