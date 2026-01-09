using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a channel that can only contain threads (forum or media channels).
/// </summary>
public interface IThreadBasedChannel : IGuildChannel, IThreadContainer
{
	/// <summary>
	/// Gets the default reaction emoji shown on the add reaction button on posts.
	/// </summary>
	DefaultReaction? DefaultReactionEmoji { get; }

	/// <summary>
	/// Gets the default sort order type used to order posts in this channel.
	/// </summary>
	int? DefaultSortOrder { get; }

	/// <summary>
	/// Gets the default forum layout view used to display posts in this channel.
	/// </summary>
	int? DefaultForumLayout { get; }

	/// <summary>
	/// Gets the default rate limit per user for threads in this channel.
	/// </summary>
	int? DefaultThreadRateLimitPerUser { get; }

	/// <summary>
	/// Gets the set of tags that can be used in this channel.
	/// </summary>
	ForumTag[]? AvailableTags { get; }

	/// <summary>
	/// Gets the channel flags (e.g., REQUIRE_TAG, HIDE_MEDIA_DOWNLOAD_OPTIONS).
	/// </summary>
	ChannelFlags? Flags { get; }
}

