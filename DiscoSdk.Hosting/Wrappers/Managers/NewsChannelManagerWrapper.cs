using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Wrapper for news channel manager operations.
/// </summary>
internal class NewsChannelManagerWrapper : MessageChannelManagerWrapper<NewsChannelManagerWrapper>, INewsChannelManager
{
	public NewsChannelManagerWrapper(DiscordId channelId, ChannelClient channelClient)
		: base(channelId, channelClient)
	{
	}

	/// <inheritdoc />
	public INewsChannelManager SetTopic(string? topic)
	{
		_changes["topic"] = topic;
		MarkAsModified("topic");
		return this;
	}

	/// <inheritdoc />
	public INewsChannelManager SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration duration)
	{
		_changes["default_auto_archive_duration"] = (int)duration;
		MarkAsModified("default_auto_archive_duration");
		return this;
	}

    INewsChannelManager IMessageChannelManager<INewsChannelManager>.SetNsfw(bool nsfw)
        => SetNsfw(nsfw);

    INewsChannelManager IMessageChannelManager<INewsChannelManager>.SetRateLimitPerUser(Slowmode? slowmode)
        => SetRateLimitPerUser(slowmode);

    INewsChannelManager IChannelManager<INewsChannelManager>.SetName(string name)
        => SetName(name);

    INewsChannelManager IChannelManager<INewsChannelManager>.SetParent(DiscordId? parentId)
        => SetParent(parentId);

    INewsChannelManager IChannelManager<INewsChannelManager>.SetPosition(int position)
        => SetPosition(position);

    INewsChannelManager IChannelManager<INewsChannelManager>.PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
        => PutPermissionOverride(targetId, isRole, allow, deny);

    INewsChannelManager IChannelManager<INewsChannelManager>.RemovePermissionOverride(ulong targetId, bool isRole)
        => RemovePermissionOverride(targetId, isRole);

    INewsChannelManager IChannelManager<INewsChannelManager>.SyncPermissions()
        => SyncPermissions();

    INewsChannelManager IManager<INewsChannelManager>.Reset()
        => Reset();

    INewsChannelManager IManager<INewsChannelManager>.Reset(string key)
        => Reset(key);

    INewsChannelManager IManager<INewsChannelManager>.Reset(params string[] keys)
        => Reset(keys);
}