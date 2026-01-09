using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Wrapper for text channel manager operations.
/// </summary>
internal class TextChannelManagerWrapper : MessageChannelManagerWrapper<TextChannelManagerWrapper>, ITextChannelManager
{
	public TextChannelManagerWrapper(DiscordId channelId, ChannelClient channelClient)
		: base(channelId, channelClient)
	{
	}

	/// <inheritdoc />
	public ITextChannelManager SetTopic(string? topic)
	{
		_changes["topic"] = topic;
		MarkAsModified("topic");
		return this;
	}

	/// <inheritdoc />
	public ITextChannelManager SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration duration)
	{
		_changes["default_auto_archive_duration"] = (int)duration;
		MarkAsModified("default_auto_archive_duration");
		return this;
	}

    ITextChannelManager IMessageChannelManager<ITextChannelManager>.SetNsfw(bool nsfw)
        => SetNsfw(nsfw);

    ITextChannelManager IMessageChannelManager<ITextChannelManager>.SetRateLimitPerUser(Slowmode? slowmode)
        => SetRateLimitPerUser(slowmode);

    ITextChannelManager IChannelManager<ITextChannelManager>.SetName(string name)
        => SetName(name);

    ITextChannelManager IChannelManager<ITextChannelManager>.SetParent(DiscordId? parentId)
        => SetParent(parentId);

    ITextChannelManager IChannelManager<ITextChannelManager>.SetPosition(int position)
        => SetPosition(position);

    ITextChannelManager IChannelManager<ITextChannelManager>.PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
        => PutPermissionOverride(targetId, isRole, allow, deny);

    ITextChannelManager IChannelManager<ITextChannelManager>.RemovePermissionOverride(ulong targetId, bool isRole)
        => RemovePermissionOverride(targetId, isRole);

    ITextChannelManager IChannelManager<ITextChannelManager>.SyncPermissions()
        => SyncPermissions();

    ITextChannelManager IManager<ITextChannelManager>.Reset()
        => Reset();

    ITextChannelManager IManager<ITextChannelManager>.Reset(string key)
        => Reset(key);

    ITextChannelManager IManager<ITextChannelManager>.Reset(params string[] keys)
        => Reset(keys);
}