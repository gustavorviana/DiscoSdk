using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord guild media channel.
/// </summary>
public interface IEditGuildMediaChannelRestAction : IRestAction<IGuildMediaChannel>
{
	/// <summary>
	/// Sets the name of the channel.
	/// </summary>
	/// <param name="name">The new name of the channel (1-100 characters).</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetName(string? name);

	/// <summary>
	/// Sets the position of the channel.
	/// </summary>
	/// <param name="position">The new position of the channel.</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetPosition(int? position);

	/// <summary>
	/// Sets the topic of the media channel.
	/// </summary>
	/// <param name="topic">The new topic of the channel (max 1024 characters).</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetTopic(string? topic);

	/// <summary>
	/// Sets the rate limit per user for messages in this channel.
	/// </summary>
	/// <param name="rateLimitPerUser">The rate limit in seconds (0-21600).</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetRateLimitPerUser(int? rateLimitPerUser);

	/// <summary>
	/// Sets the permission overwrites for the channel.
	/// </summary>
	/// <param name="permissionOverwrites">The permission overwrites.</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetPermissionOverwrites(PermissionOverwrite[]? permissionOverwrites);

	/// <summary>
	/// Sets the parent category channel for the channel.
	/// </summary>
	/// <param name="parentId">The ID of the parent category channel, or null to remove the parent.</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetParentId(DiscordId? parentId);

	/// <summary>
	/// Sets the default auto-archive duration for threads created in this channel.
	/// </summary>
	/// <param name="duration">The duration in minutes (60, 1440, 4320, 10080).</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetDefaultAutoArchiveDuration(int? duration);

	/// <summary>
	/// Sets the channel flags.
	/// </summary>
	/// <param name="flags">The channel flags.</param>
	/// <returns>The current <see cref="IEditGuildMediaChannelRestAction"/> instance.</returns>
	IEditGuildMediaChannelRestAction SetFlags(ChannelFlags? flags);
}

