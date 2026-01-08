using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord guild stage voice channel.
/// </summary>
public interface IEditGuildStageVoiceChannelRestAction : IRestAction<IGuildStageVoiceChannel>
{
	/// <summary>
	/// Sets the name of the channel.
	/// </summary>
	/// <param name="name">The new name of the channel (1-100 characters).</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetName(string? name);

	/// <summary>
	/// Sets the position of the channel.
	/// </summary>
	/// <param name="position">The new position of the channel.</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetPosition(int? position);

	/// <summary>
	/// Sets the topic of the stage channel.
	/// </summary>
	/// <param name="topic">The new topic of the channel (max 120 characters).</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetTopic(string? topic);

	/// <summary>
	/// Sets the bitrate of the voice channel.
	/// </summary>
	/// <param name="bitrate">The bitrate in bits per second (8000-96000 or 128000 for VIP servers).</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetBitrate(int? bitrate);

	/// <summary>
	/// Sets the user limit of the voice channel.
	/// </summary>
	/// <param name="userLimit">The maximum number of users (0 for unlimited).</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetUserLimit(int? userLimit);

	/// <summary>
	/// Sets the permission overwrites for the channel.
	/// </summary>
	/// <param name="permissionOverwrites">The permission overwrites.</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetPermissionOverwrites(PermissionOverwrite[]? permissionOverwrites);

	/// <summary>
	/// Sets the parent category channel for the channel.
	/// </summary>
	/// <param name="parentId">The ID of the parent category channel, or null to remove the parent.</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetParentId(DiscordId? parentId);

	/// <summary>
	/// Sets the RTC region for the voice channel.
	/// </summary>
	/// <param name="rtcRegion">The RTC region ID, or null for automatic.</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetRtcRegion(string? rtcRegion);

	/// <summary>
	/// Sets the video quality mode for the voice channel.
	/// </summary>
	/// <param name="videoQualityMode">The video quality mode.</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetVideoQualityMode(VideoQualityMode? videoQualityMode);

	/// <summary>
	/// Sets the channel flags.
	/// </summary>
	/// <param name="flags">The channel flags.</param>
	/// <returns>The current <see cref="IEditGuildStageVoiceChannelRestAction"/> instance.</returns>
	IEditGuildStageVoiceChannelRestAction SetFlags(ChannelFlags? flags);
}

