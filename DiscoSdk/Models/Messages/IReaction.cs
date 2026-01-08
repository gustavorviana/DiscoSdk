namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a reaction to a message with operations.
/// </summary>
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

