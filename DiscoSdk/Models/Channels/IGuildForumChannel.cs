using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord guild forum channel.
/// </summary>
public interface IGuildForumChannel : IGuildChannel
{
	/// <summary>
	/// Gets the default duration for newly created threads.
	/// </summary>
	int? DefaultAutoArchiveDuration { get; }

	/// <summary>
	/// Gets the default forum layout view used to display posts in this forum channel.
	/// </summary>
	int? DefaultForumLayout { get; }

	/// <summary>
	/// Gets the default reaction emoji shown on the add reaction button on forum posts.
	/// </summary>
	DefaultReaction? DefaultReactionEmoji { get; }

	/// <summary>
	/// Gets the default sort order type used to order posts in forum channels.
	/// </summary>
	int? DefaultSortOrder { get; }

	/// <summary>
	/// Gets the default rate limit per user for threads in this forum channel.
	/// </summary>
	int? DefaultThreadRateLimitPerUser { get; }

	/// <summary>
	/// Gets the available tags for this forum channel.
	/// </summary>
	ForumTag[]? AvailableTags { get; }

	/// <summary>
	/// Gets the topic of the forum channel.
	/// </summary>
	string? Topic { get; }

	/// <summary>
	/// Gets the ID of the last message sent in this channel (in threads).
	/// </summary>
	DiscordId? LastMessageId { get; }

	/// <summary>
	/// Gets the rate limit per user for posts in this forum.
	/// </summary>
	int? RateLimitPerUser { get; }

	/// <summary>
	/// Gets when the last pinned message was pinned.
	/// </summary>
	string? LastPinTimestamp { get; }

	/// <summary>
	/// Gets the active threads in this forum channel.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of active thread channels.</returns>
	Task<IChannel[]> GetActiveThreadsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the public archived threads in this forum channel.
	/// </summary>
	/// <param name="before">Get threads before this timestamp.</param>
	/// <param name="limit">Maximum number of threads to return (1-100, default 50).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of archived thread channels.</returns>
	Task<IChannel[]> GetPublicArchivedThreadsAsync(string? before = null, int? limit = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the private archived threads in this forum channel (requires MANAGE_THREADS permission).
	/// </summary>
	/// <param name="before">Get threads before this timestamp.</param>
	/// <param name="limit">Maximum number of threads to return (1-100, default 50).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of archived thread channels.</returns>
	Task<IChannel[]> GetPrivateArchivedThreadsAsync(string? before = null, int? limit = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a builder for editing this channel.
	/// </summary>
	/// <returns>An <see cref="IEditGuildForumChannelRestAction"/> instance for editing the channel.</returns>
	IEditGuildForumChannelRestAction Edit();

	/// <summary>
	/// Creates a builder for creating a new post in this forum channel.
	/// </summary>
	/// <param name="name">The name of the post (thread).</param>
	/// <param name="content">The initial content of the post message.</param>
	/// <returns>An <see cref="ICreateForumPostRestAction"/> instance for creating the post.</returns>
	ICreateForumPostRestAction CreatePostBuilder(string name, string? content = null);
}

