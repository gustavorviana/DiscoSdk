using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Wrapper for voice channel manager operations.
/// </summary>
internal class VoiceChannelManagerWrapper : ChannelManagerWrapper<VoiceChannelManagerWrapper>, IVoiceChannelManager
{
	public VoiceChannelManagerWrapper(Snowflake channelId, ChannelClient channelClient)
		: base(channelId, channelClient)
	{
	}

	/// <inheritdoc />
	public IVoiceChannelManager SetBitrate(int bitrate)
	{
		_changes["bitrate"] = bitrate;
		MarkAsModified("bitrate");
		return this;
	}

	/// <inheritdoc />
	public IVoiceChannelManager SetUserLimit(int userLimit)
	{
		_changes["user_limit"] = userLimit;
		MarkAsModified("user_limit");
		return this;
	}

	/// <inheritdoc />
	public IVoiceChannelManager SetRegion(string? regionId)
	{
		_changes["rtc_region"] = regionId;
		MarkAsModified("rtc_region");
		return this;
	}

    IVoiceChannelManager IChannelManager<IVoiceChannelManager>.SetName(string name)
        => SetName(name);

    IVoiceChannelManager IChannelManager<IVoiceChannelManager>.SetParent(Snowflake? parentId)
        => SetParent(parentId);

    IVoiceChannelManager IChannelManager<IVoiceChannelManager>.SetPosition(int position)
        => SetPosition(position);

    IVoiceChannelManager IChannelManager<IVoiceChannelManager>.PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
        => PutPermissionOverride(targetId, isRole, allow, deny);

    IVoiceChannelManager IChannelManager<IVoiceChannelManager>.RemovePermissionOverride(ulong targetId, bool isRole)
        => RemovePermissionOverride(targetId, isRole);

    IVoiceChannelManager IChannelManager<IVoiceChannelManager>.SyncPermissions()
        => SyncPermissions();

    IVoiceChannelManager IManager<IVoiceChannelManager>.Reset()
        => Reset();

    IVoiceChannelManager IManager<IVoiceChannelManager>.Reset(string key)
        => Reset(key);

    IVoiceChannelManager IManager<IVoiceChannelManager>.Reset(params string[] keys)
        => Reset(keys);
}