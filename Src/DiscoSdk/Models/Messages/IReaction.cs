namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a reaction to a message with operations.
/// </summary>
/// <remarks>
/// <see cref="IDeletable.Delete"/> on a reaction requires
/// <see cref="DiscordIntent.GuildMessageReactions"/> for guild messages or
/// <see cref="DiscordIntent.DirectMessageReactions"/> for DMs. Without the matching intent,
/// executing the returned action throws <see cref="DiscoSdk.Exceptions.MissingIntentException"/>.
/// </remarks>
public interface IReaction : IDeletable
{
	/// <summary>
	/// Gets the number of times this emoji has been used to react.
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Gets a value indicating whether the current user reacted using this emoji.
	/// </summary>
	bool Me { get; }

	/// <summary>
	/// Gets the emoji information.
	/// </summary>
	Emoji Emoji { get; }
}

