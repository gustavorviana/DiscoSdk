using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Wrapper for guild channel manager operations.
/// </summary>
internal class GuildChannelManagerWrapper : ChannelManagerWrapper<GuildChannelManagerWrapper>, IGuildChannelManager
{
	public GuildChannelManagerWrapper(Snowflake channelId, ChannelClient channelClient)
		: base(channelId, channelClient)
	{
	}

    IGuildChannelManager IChannelManager<IGuildChannelManager>.PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
        => PutPermissionOverride(targetId, isRole, allow, deny);

    IGuildChannelManager IChannelManager<IGuildChannelManager>.RemovePermissionOverride(ulong targetId, bool isRole)
        => RemovePermissionOverride(targetId, isRole);

    IGuildChannelManager IManager<IGuildChannelManager>.Reset()
        => Reset();

    IGuildChannelManager IManager<IGuildChannelManager>.Reset(string key)
        => Reset(key);

    IGuildChannelManager IManager<IGuildChannelManager>.Reset(params string[] keys)
        => Reset(keys);

    IGuildChannelManager IChannelManager<IGuildChannelManager>.SetName(string name)
        => SetName(name);

    IGuildChannelManager IChannelManager<IGuildChannelManager>.SetParent(Snowflake? parentId)
        => SetParent(parentId);

    IGuildChannelManager IChannelManager<IGuildChannelManager>.SetPosition(int position)
        => SetPosition(position);

    IGuildChannelManager IChannelManager<IGuildChannelManager>.SyncPermissions()
        => SyncPermissions();
}