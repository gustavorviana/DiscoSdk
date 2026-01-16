using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Wrapper for thread channel manager operations.
/// </summary>
internal class ThreadChannelManagerWrapper : MessageChannelManagerWrapper<ThreadChannelManagerWrapper>, IThreadChannelManager
{
	public ThreadChannelManagerWrapper(Snowflake channelId, ChannelClient channelClient)
		: base(channelId, channelClient)
	{
	}

	/// <inheritdoc />
	public IThreadChannelManager SetArchived(bool archived)
	{
		_changes["archived"] = archived;
		MarkAsModified("archived");
		return this;
	}

	/// <inheritdoc />
	public IThreadChannelManager SetLocked(bool locked)
	{
		_changes["locked"] = locked;
		MarkAsModified("locked");
		return this;
	}

	/// <inheritdoc />
	public IThreadChannelManager SetInvitable(bool invitable)
	{
		_changes["invitable"] = invitable;
		MarkAsModified("invitable");
		return this;
	}

	/// <inheritdoc />
	public IThreadChannelManager SetAutoArchiveDuration(ThreadAutoArchiveDuration duration)
	{
		_changes["auto_archive_duration"] = (int)duration;
		MarkAsModified("auto_archive_duration");
		return this;
	}

	/// <inheritdoc />
	public IThreadChannelManager SetSlowmode(Slowmode? slowmode)
	{
		_changes["rate_limit_per_user"] = slowmode?.Seconds ?? 0;
		MarkAsModified("rate_limit_per_user");
		return this;
	}

    IThreadChannelManager IMessageChannelManager<IThreadChannelManager>.SetNsfw(bool nsfw)
		=> SetNsfw(nsfw);

    IThreadChannelManager IMessageChannelManager<IThreadChannelManager>.SetRateLimitPerUser(Slowmode? slowmode)
		=> SetRateLimitPerUser(slowmode);

    IThreadChannelManager IChannelManager<IThreadChannelManager>.SetName(string name)
		=> SetName(name);

    IThreadChannelManager IChannelManager<IThreadChannelManager>.SetParent(Snowflake? parentId)
		=> SetParent(parentId);

    IThreadChannelManager IChannelManager<IThreadChannelManager>.SetPosition(int position)
		=> SetPosition(position);

    IThreadChannelManager IChannelManager<IThreadChannelManager>.PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
		=> PutPermissionOverride(targetId, isRole, allow, deny);

    IThreadChannelManager IChannelManager<IThreadChannelManager>.RemovePermissionOverride(ulong targetId, bool isRole)
		=> RemovePermissionOverride(targetId, isRole);

    IThreadChannelManager IChannelManager<IThreadChannelManager>.SyncPermissions()
		=> SyncPermissions();

    IThreadChannelManager IManager<IThreadChannelManager>.Reset()
		=> Reset();

    IThreadChannelManager IManager<IThreadChannelManager>.Reset(string key)
		=> Reset(key);

    IThreadChannelManager IManager<IThreadChannelManager>.Reset(params string[] keys)
		=> Reset(keys);
}