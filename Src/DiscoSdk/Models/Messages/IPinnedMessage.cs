namespace DiscoSdk.Models.Messages;

/// <summary>
/// A pinned message together with the moment it was pinned.
/// </summary>
public interface IPinnedMessage
{
	/// <summary>
	/// Gets when the message was pinned. Pass it to
	/// <see cref="Rest.Actions.IPinnedMessagePaginationAction.Before"/> to fetch the next page.
	/// </summary>
	DateTimeOffset PinnedAt { get; }

	/// <summary>
	/// Gets the pinned message.
	/// </summary>
	IMessage Message { get; }
}
