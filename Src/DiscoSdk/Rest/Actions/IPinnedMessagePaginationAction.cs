using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving the pinned messages of a channel, newest pin first.
/// </summary>
/// <remarks>
/// Each page returns at most <see cref="IPaginationAction{TItem, TSelf}.Limit"/> items (1–50,
/// Discord's default is 50). To read the next page, call <see cref="Before"/> with the
/// <see cref="IPinnedMessage.PinnedAt"/> of the last item; a page shorter than the limit is the last one.
/// Without <see cref="Models.Enums.DiscordPermission.ReadMessageHistory"/> Discord returns an empty list.
/// </remarks>
public interface IPinnedMessagePaginationAction : IPaginationAction<IPinnedMessage, IPinnedMessagePaginationAction>
{
	/// <summary>
	/// Gets messages pinned before the specified moment.
	/// </summary>
	/// <param name="pinnedBefore">Only messages pinned before this moment are returned.</param>
	/// <returns>The current <see cref="IPinnedMessagePaginationAction"/> instance.</returns>
	IPinnedMessagePaginationAction Before(DateTimeOffset pinnedBefore);
}
