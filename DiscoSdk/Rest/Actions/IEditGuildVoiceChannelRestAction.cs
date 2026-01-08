using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord guild voice channel.
/// </summary>
public interface IEditGuildVoiceChannelRestAction : IRestAction<IGuildVoiceChannel>
{
	/// <summary>
	/// Sets the name of the channel.
	/// </summary>
	/// <param name="name">The new name of the channel (1-100 characters).</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetName(string? name);

	/// <summary>
	/// Sets the position of the channel.
	/// </summary>
	/// <param name="position">The new position of the channel.</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetPosition(int? position);

	/// <summary>
	/// Sets the bitrate of the voice channel.
	/// </summary>
	/// <param name="bitrate">The bitrate in bits per second (8000-96000 or 128000 for VIP servers).</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetBitrate(int? bitrate);

	/// <summary>
	/// Sets the user limit of the voice channel.
	/// </summary>
	/// <param name="userLimit">The maximum number of users (0 for unlimited).</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetUserLimit(int? userLimit);

	/// <summary>
	/// Sets the permission overwrites for the channel.
	/// </summary>
	/// <param name="permissionOverwrites">The permission overwrites.</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetPermissionOverwrites(PermissionOverwrite[]? permissionOverwrites);

	/// <summary>
	/// Sets the parent category channel for the channel.
	/// </summary>
	/// <param name="parentId">The ID of the parent category channel, or null to remove the parent.</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetParentId(DiscordId? parentId);

	/// <summary>
	/// Sets the RTC region for the voice channel.
	/// </summary>
	/// <param name="rtcRegion">The RTC region ID, or null for automatic.</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetRtcRegion(string? rtcRegion);

	/// <summary>
	/// Sets the video quality mode for the voice channel.
	/// </summary>
	/// <param name="videoQualityMode">The video quality mode.</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetVideoQualityMode(VideoQualityMode? videoQualityMode);

	/// <summary>
	/// Sets the channel flags.
	/// </summary>
	/// <param name="flags">The channel flags.</param>
	/// <returns>The current <see cref="IEditGuildVoiceChannelRestAction"/> instance.</returns>
	IEditGuildVoiceChannelRestAction SetFlags(ChannelFlags? flags);
}

