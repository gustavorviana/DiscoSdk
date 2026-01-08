using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord guild forum channel.
/// </summary>
public interface IEditGuildForumChannelRestAction : IRestAction<IGuildForumChannel>
{
	/// <summary>
	/// Sets the name of the channel.
	/// </summary>
	/// <param name="name">The new name of the channel (1-100 characters).</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetName(string? name);

	/// <summary>
	/// Sets the position of the channel.
	/// </summary>
	/// <param name="position">The new position of the channel.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetPosition(int? position);

	/// <summary>
	/// Sets the topic of the forum channel.
	/// </summary>
	/// <param name="topic">The new topic of the channel (max 1024 characters).</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetTopic(string? topic);

	/// <summary>
	/// Sets whether the channel is NSFW.
	/// </summary>
	/// <param name="nsfw">Whether the channel should be NSFW.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetNsfw(bool? nsfw);

	/// <summary>
	/// Sets the rate limit per user for posts in this forum.
	/// </summary>
	/// <param name="rateLimitPerUser">The rate limit in seconds (0-21600).</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetRateLimitPerUser(int? rateLimitPerUser);

	/// <summary>
	/// Sets the permission overwrites for the channel.
	/// </summary>
	/// <param name="permissionOverwrites">The permission overwrites.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetPermissionOverwrites(PermissionOverwrite[]? permissionOverwrites);

	/// <summary>
	/// Sets the parent category channel for the channel.
	/// </summary>
	/// <param name="parentId">The ID of the parent category channel, or null to remove the parent.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetParentId(DiscordId? parentId);

	/// <summary>
	/// Sets the default auto-archive duration for threads created in this forum channel.
	/// </summary>
	/// <param name="duration">The duration in minutes (60, 1440, 4320, 10080).</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetDefaultAutoArchiveDuration(int? duration);

	/// <summary>
	/// Sets the default forum layout for the forum channel.
	/// </summary>
	/// <param name="layout">The forum layout (0 = not set, 1 = list view, 2 = gallery view).</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetDefaultForumLayout(int? layout);

	/// <summary>
	/// Sets the available tags for the forum channel.
	/// </summary>
	/// <param name="tags">The available tags.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetAvailableTags(ForumTag[]? tags);

	/// <summary>
	/// Sets the default reaction emoji for the forum channel.
	/// </summary>
	/// <param name="emojiId">The ID of the custom emoji, or null to use unicode.</param>
	/// <param name="emojiName">The unicode emoji name, or null to use custom emoji.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetDefaultReactionEmoji(DiscordId? emojiId, string? emojiName);

	/// <summary>
	/// Sets the default sort order for the forum channel.
	/// </summary>
	/// <param name="sortOrder">The sort order (0 = latest activity, 1 = creation date).</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetDefaultSortOrder(int? sortOrder);

	/// <summary>
	/// Sets the default thread rate limit per user for the forum channel.
	/// </summary>
	/// <param name="rateLimitPerUser">The rate limit in seconds.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetDefaultThreadRateLimitPerUser(int? rateLimitPerUser);

	/// <summary>
	/// Sets the channel flags.
	/// </summary>
	/// <param name="flags">The channel flags.</param>
	/// <returns>The current <see cref="IEditGuildForumChannelRestAction"/> instance.</returns>
	IEditGuildForumChannelRestAction SetFlags(ChannelFlags? flags);
}

