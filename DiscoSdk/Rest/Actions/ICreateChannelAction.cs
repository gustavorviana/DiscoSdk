using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for creating a Discord channel.
/// </summary>
public interface ICreateChannelAction : IRestAction<IGuildChannel>
{
    /// <summary>
    /// Sets the channel's name.
    /// </summary>
    /// <param name="name">The channel's name.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetName(string name);

    /// <summary>
    /// Sets the channel's type.
    /// </summary>
    /// <param name="type">The channel's type.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetType(ChannelType type);

    /// <summary>
    /// Sets the channel's topic.
    /// </summary>
    /// <param name="topic">The channel's topic, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetTopic(string? topic);

    /// <summary>
    /// Sets whether the channel is NSFW.
    /// </summary>
    /// <param name="nsfw">True if the channel is NSFW, false otherwise, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetNsfw(bool? nsfw = true);

    /// <summary>
    /// Sets the bitrate for voice channels.
    /// </summary>
    /// <param name="bitrate">The bitrate in bits per second, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetBitrate(int? bitrate);

    /// <summary>
    /// Sets the user limit for voice channels.
    /// </summary>
    /// <param name="userLimit">The maximum number of users, or null for unlimited.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetUserLimit(int? userLimit);

    /// <summary>
    /// Sets the rate limit per user (slowmode) in seconds.
    /// </summary>
    /// <param name="rateLimit">The rate limit in seconds, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetRateLimitPerUser(int? rateLimit);

    /// <summary>
    /// Sets the parent category channel ID.
    /// </summary>
    /// <param name="parentId">The parent category channel ID, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetParentId(DiscordId? parentId);

    /// <summary>
    /// Sets the channel's position.
    /// </summary>
    /// <param name="position">The channel's position, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetPosition(int? position);

    /// <summary>
    /// Sets the permission overwrites for the channel.
    /// </summary>
    /// <param name="overwrites">The permission overwrites, or null to not set them.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetPermissionOverwrites(PermissionOverwrite[]? overwrites);

    /// <summary>
    /// Sets the RTC region for voice channels.
    /// </summary>
    /// <param name="region">The RTC region, or null for automatic.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetRtcRegion(string? region);

    /// <summary>
    /// Sets the video quality mode for voice channels.
    /// </summary>
    /// <param name="mode">The video quality mode, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetVideoQualityMode(VideoQualityMode? mode);

    /// <summary>
    /// Sets the default auto archive duration for threads created in this channel.
    /// </summary>
    /// <param name="duration">The auto archive duration in minutes, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetDefaultAutoArchiveDuration(int? duration);

    /// <summary>
    /// Sets the default reaction emoji for forum posts.
    /// </summary>
    /// <param name="emoji">The emoji string (e.g., "👍" or "custom_emoji_id"), or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetDefaultReactionEmoji(string? emoji);

    /// <summary>
    /// Sets the default thread rate limit per user in seconds.
    /// </summary>
    /// <param name="rateLimit">The rate limit in seconds, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetDefaultThreadRateLimitPerUser(int? rateLimit);

    /// <summary>
    /// Sets the default sort order for forum channels.
    /// </summary>
    /// <param name="sortOrder">The sort order, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetDefaultSortOrder(SortOrderType? sortOrder);

    /// <summary>
    /// Sets the default forum layout for forum channels.
    /// </summary>
    /// <param name="layout">The forum layout, or null to not set it.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetDefaultForumLayout(ForumLayoutType? layout);

    /// <summary>
    /// Sets the available tags for forum channels.
    /// </summary>
    /// <param name="tags">The available tags, or null to not set them.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetAvailableTags(ForumTag[]? tags);

    /// <summary>
    /// Sets the channel flags.
    /// </summary>
    /// <param name="flags">The channel flags, or null to not set them.</param>
    /// <returns>The current <see cref="ICreateChannelAction"/> instance.</returns>
    ICreateChannelAction SetFlags(ChannelFlags? flags);
}

