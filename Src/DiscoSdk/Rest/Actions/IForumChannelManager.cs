using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for forum channel operations.
/// </summary>
public interface IForumChannelManager : IMessageChannelManager<IForumChannelManager>
{
	/// <summary>
	/// Sets the topic of the forum channel.
	/// </summary>
	/// <param name="topic">The new topic, or null to remove the topic.</param>
	/// <returns>The current <see cref="IForumChannelManager"/> instance.</returns>
	IForumChannelManager SetTopic(string? topic);

	/// <summary>
	/// Sets the default reaction emoji shown on the add reaction button on forum posts.
	/// </summary>
	/// <param name="emoji">The emoji string (e.g., "🔥" or custom emoji ID), or null to remove.</param>
	/// <returns>The current <see cref="IForumChannelManager"/> instance.</returns>
	IForumChannelManager SetDefaultReactionEmoji(string? emoji);

	/// <summary>
	/// Sets the default auto-archive duration for threads created in this channel.
	/// </summary>
	/// <param name="duration">The auto-archive duration.</param>
	/// <returns>The current <see cref="IForumChannelManager"/> instance.</returns>
	IForumChannelManager SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration duration);

	/// <summary>
	/// Sets the default sort order type used to order posts in this forum channel.
	/// </summary>
	/// <param name="sortOrder">The sort order, or null to use default.</param>
	/// <returns>The current <see cref="IForumChannelManager"/> instance.</returns>
	IForumChannelManager SetDefaultSortOrder(int? sortOrder);

	/// <summary>
	/// Sets the default forum layout view used to display posts in this channel.
	/// </summary>
	/// <param name="layout">The forum layout, or null to use default.</param>
	/// <returns>The current <see cref="IForumChannelManager"/> instance.</returns>
	IForumChannelManager SetDefaultLayout(int? layout);

	/// <summary>
	/// Sets the available tags that can be used in this forum channel.
	/// </summary>
	/// <param name="tags">The available tags.</param>
	/// <returns>The current <see cref="IForumChannelManager"/> instance.</returns>
	IForumChannelManager SetAvailableTags(IReadOnlyList<string> tags);
}

