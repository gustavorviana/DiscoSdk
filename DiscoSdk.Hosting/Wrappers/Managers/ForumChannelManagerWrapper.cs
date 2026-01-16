using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Wrapper for forum channel manager operations.
/// </summary>
internal class ForumChannelManagerWrapper : MessageChannelManagerWrapper<ForumChannelManagerWrapper>, IForumChannelManager
{
    public ForumChannelManagerWrapper(Snowflake channelId, ChannelClient channelClient)
        : base(channelId, channelClient)
    {
    }

    /// <inheritdoc />
    public IForumChannelManager SetTopic(string? topic)
    {
        _changes["topic"] = topic;
        MarkAsModified("topic");
        return this;
    }

    /// <inheritdoc />
    public IForumChannelManager SetDefaultReactionEmoji(string? emoji)
    {
        _changes["default_reaction_emoji"] = emoji != null ? new { emoji_id = emoji, emoji_name = emoji } : null;
        MarkAsModified("default_reaction_emoji");
        return this;
    }

    /// <inheritdoc />
    public IForumChannelManager SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration duration)
    {
        _changes["default_auto_archive_duration"] = (int)duration;
        MarkAsModified("default_auto_archive_duration");
        return this;
    }

    /// <inheritdoc />
    public IForumChannelManager SetDefaultSortOrder(int? sortOrder)
    {
        _changes["default_sort_order"] = sortOrder;
        MarkAsModified("default_sort_order");
        return this;
    }

    /// <inheritdoc />
    public IForumChannelManager SetDefaultLayout(int? layout)
    {
        _changes["default_forum_layout"] = layout;
        MarkAsModified("default_forum_layout");
        return this;
    }

    /// <inheritdoc />
    public IForumChannelManager SetAvailableTags(IReadOnlyList<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        _changes["available_tags"] = tags.Select(t => new { name = t }).ToArray();
        MarkAsModified("available_tags");
        return this;
    }

    IForumChannelManager IForumChannelManager.SetTopic(string? topic)
        => SetTopic(topic);

    IForumChannelManager IForumChannelManager.SetDefaultReactionEmoji(string? emoji)
        => SetDefaultReactionEmoji(emoji);

    IForumChannelManager IForumChannelManager.SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration duration)
        => SetDefaultAutoArchiveDuration(duration);

    IForumChannelManager IForumChannelManager.SetDefaultSortOrder(int? sortOrder)
        => SetDefaultSortOrder(sortOrder);

    IForumChannelManager IForumChannelManager.SetDefaultLayout(int? layout)
        => SetDefaultLayout(layout);

    IForumChannelManager IForumChannelManager.SetAvailableTags(IReadOnlyList<string> tags)
        => SetAvailableTags(tags);

    IForumChannelManager IMessageChannelManager<IForumChannelManager>.SetNsfw(bool nsfw)
        => SetNsfw(nsfw);

    IForumChannelManager IMessageChannelManager<IForumChannelManager>.SetRateLimitPerUser(Slowmode? slowmode)
        => SetRateLimitPerUser(slowmode);

    IForumChannelManager IChannelManager<IForumChannelManager>.SetName(string name)
        => SetName(name);

    IForumChannelManager IChannelManager<IForumChannelManager>.SetParent(Snowflake? parentId)
        => SetParent(parentId);

    IForumChannelManager IChannelManager<IForumChannelManager>.SetPosition(int position)
        => SetPosition(position);

    IForumChannelManager IChannelManager<IForumChannelManager>.PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny)
        => PutPermissionOverride(targetId, isRole, allow, deny);

    IForumChannelManager IChannelManager<IForumChannelManager>.RemovePermissionOverride(ulong targetId, bool isRole)
        => RemovePermissionOverride(targetId, isRole);

    IForumChannelManager IChannelManager<IForumChannelManager>.SyncPermissions()
        => SyncPermissions();

    IForumChannelManager IManager<IForumChannelManager>.Reset()
        => Reset();

    IForumChannelManager IManager<IForumChannelManager>.Reset(string key)
        => Reset(key);

    IForumChannelManager IManager<IForumChannelManager>.Reset(params string[] keys)
        => Reset(keys);
}