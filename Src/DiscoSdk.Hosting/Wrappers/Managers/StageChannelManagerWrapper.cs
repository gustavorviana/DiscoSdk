using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Wrapper for stage channel manager operations.
/// </summary>
internal class StageChannelManagerWrapper : ChannelManagerWrapper<StageChannelManagerWrapper>, IStageChannelManager
{
	public StageChannelManagerWrapper(Snowflake channelId, ChannelClient channelClient)
		: base(channelId, channelClient)
	{
	}

	/// <inheritdoc />
	public IStageChannelManager SetBitrate(int bitrate)
	{
		_changes["bitrate"] = bitrate;
		MarkAsModified("bitrate");
		return this;
	}

	/// <inheritdoc />
	public IStageChannelManager SetRegion(string? regionId)
	{
		_changes["rtc_region"] = regionId;
		MarkAsModified("rtc_region");
		return this;
	}

    IStageChannelManager IChannelManager<IStageChannelManager>.PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
        => PutPermissionOverride(targetId, isRole, allow, deny);

    IStageChannelManager IChannelManager<IStageChannelManager>.RemovePermissionOverride(ulong targetId, bool isRole)
        => RemovePermissionOverride(targetId, isRole);

    IStageChannelManager IManager<IStageChannelManager>.Reset()
        => Reset();

    IStageChannelManager IManager<IStageChannelManager>.Reset(string key)
        => Reset(key);

    IStageChannelManager IManager<IStageChannelManager>.Reset(params string[] keys)
        => Reset(keys);

    IStageChannelManager IChannelManager<IStageChannelManager>.SetName(string name)
        => SetName(name);

    IStageChannelManager IChannelManager<IStageChannelManager>.SetParent(Snowflake? parentId)
        => SetParent(parentId);

    IStageChannelManager IChannelManager<IStageChannelManager>.SetPosition(int position)
        => SetPosition(position);

    IStageChannelManager IChannelManager<IStageChannelManager>.SyncPermissions()
        => SyncPermissions();
}