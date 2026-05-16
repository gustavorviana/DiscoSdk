using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a channel that can only contain threads (forum or media channels).
/// </summary>
public interface IThreadBasedChannel : IStandardGuildChannel, IThreadContainer
{
	/// <summary>
	/// Gets the default reaction emoji shown on the add reaction button on posts.
	/// </summary>
	IDefaultReaction? DefaultReactionEmoji { get; }

	/// <summary>
	/// Gets the default sort order used to order posts in this channel, or <c>null</c> if Discord
	/// has not set one (in which case it behaves as <see cref="SortOrderType.LatestActivity"/>).
	/// </summary>
	SortOrderType? DefaultSortOrder { get; }

	/// <summary>
	/// Gets the set of tags that can be used in this channel.
	/// </summary>
	IReadOnlyList<IForumTag>? AvailableTags { get; }

	/// <summary>
	/// Gets the channel flags (e.g., REQUIRE_TAG, HIDE_MEDIA_DOWNLOAD_OPTIONS).
	/// </summary>
	ChannelFlags? Flags { get; }
}

