using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord guild text channel.
/// </summary>
public interface IEditGuildTextChannelRestAction : IRestAction<IGuildTextChannel>
{
	/// <summary>
	/// Sets the name of the channel.
	/// </summary>
	/// <param name="name">The new name of the channel (1-100 characters).</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetName(string? name);

	/// <summary>
	/// Sets the position of the channel.
	/// </summary>
	/// <param name="position">The new position of the channel.</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetPosition(int? position);

	/// <summary>
	/// Sets the topic of the channel.
	/// </summary>
	/// <param name="topic">The new topic of the channel (max 1024 characters).</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetTopic(string? topic);

	/// <summary>
	/// Sets whether the channel is NSFW.
	/// </summary>
	/// <param name="nsfw">Whether the channel should be NSFW.</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetNsfw(bool? nsfw);

	/// <summary>
	/// Sets the rate limit per user for the channel.
	/// </summary>
	/// <param name="rateLimitPerUser">The rate limit in seconds (0-21600).</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetRateLimitPerUser(int? rateLimitPerUser);

	/// <summary>
	/// Sets the permission overwrites for the channel.
	/// </summary>
	/// <param name="permissionOverwrites">The permission overwrites.</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetPermissionOverwrites(PermissionOverwrite[]? permissionOverwrites);

	/// <summary>
	/// Sets the parent category channel for the channel.
	/// </summary>
	/// <param name="parentId">The ID of the parent category channel, or null to remove the parent.</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetParentId(DiscordId? parentId);

	/// <summary>
	/// Sets the default auto-archive duration for threads created in this channel.
	/// </summary>
	/// <param name="duration">The duration in minutes (60, 1440, 4320, 10080).</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetDefaultAutoArchiveDuration(int? duration);

	/// <summary>
	/// Sets the channel flags.
	/// </summary>
	/// <param name="flags">The channel flags.</param>
	/// <returns>The current <see cref="IEditGuildTextChannelRestAction"/> instance.</returns>
	IEditGuildTextChannelRestAction SetFlags(ChannelFlags? flags);
}

