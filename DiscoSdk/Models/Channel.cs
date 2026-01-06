using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord channel.
/// </summary>
public class Channel
{
    /// <summary>
    /// Gets or sets the channel's unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of channel.
    /// </summary>
    [JsonPropertyName("type")]
    public ChannelType Type { get; set; }

    /// <summary>
    /// Gets or sets the ID of the guild this channel belongs to.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the sorting position of the channel.
    /// </summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }

    /// <summary>
    /// Gets or sets the explicit permission overwrites for members and roles.
    /// </summary>
    [JsonPropertyName("permission_overwrites")]
    public List<PermissionOverwrite>? PermissionOverwrites { get; set; }

    /// <summary>
    /// Gets or sets the name of the channel.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the topic of the channel.
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the channel is NSFW.
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool? Nsfw { get; set; }

    /// <summary>
    /// Gets or sets the ID of the last message sent in this channel.
    /// </summary>
    [JsonPropertyName("last_message_id")]
    public string? LastMessageId { get; set; }

    /// <summary>
    /// Gets or sets the bitrate of the voice channel.
    /// </summary>
    [JsonPropertyName("bitrate")]
    public int? Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the user limit of the voice channel.
    /// </summary>
    [JsonPropertyName("user_limit")]
    public int? UserLimit { get; set; }

    /// <summary>
    /// Gets or sets the amount of seconds a user has to wait before sending another message.
    /// </summary>
    [JsonPropertyName("rate_limit_per_user")]
    public int? RateLimitPerUser { get; set; }

    /// <summary>
    /// Gets or sets the recipients of the DM.
    /// </summary>
    [JsonPropertyName("recipients")]
    public List<User>? Recipients { get; set; }

    /// <summary>
    /// Gets or sets the icon hash of the group DM.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the ID of the creator of the group DM or thread.
    /// </summary>
    [JsonPropertyName("owner_id")]
    public string? OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the application ID of the group DM creator if it is bot-created.
    /// </summary>
    [JsonPropertyName("application_id")]
    public string? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the parent channel for the channel.
    /// </summary>
    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets when the last pinned message was pinned.
    /// </summary>
    [JsonPropertyName("last_pin_timestamp")]
    public string? LastPinTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the voice region ID for the voice channel.
    /// </summary>
    [JsonPropertyName("rtc_region")]
    public string? RtcRegion { get; set; }

    /// <summary>
    /// Gets or sets the camera video quality mode of the voice channel.
    /// </summary>
    [JsonPropertyName("video_quality_mode")]
    public VideoQualityMode? VideoQualityMode { get; set; }

    /// <summary>
    /// Gets or sets the number of messages in a thread.
    /// </summary>
    [JsonPropertyName("message_count")]
    public int? MessageCount { get; set; }

    /// <summary>
    /// Gets or sets the approximate member count of the thread.
    /// </summary>
    [JsonPropertyName("member_count")]
    public int? MemberCount { get; set; }

    /// <summary>
    /// Gets or sets thread-specific fields not needed by other channels.
    /// </summary>
    [JsonPropertyName("thread_metadata")]
    public ThreadMetadata? ThreadMetadata { get; set; }

    /// <summary>
    /// Gets or sets thread member object for the current user, if they have joined the thread.
    /// </summary>
    [JsonPropertyName("member")]
    public ThreadMember? Member { get; set; }

    /// <summary>
    /// Gets or sets the default duration for newly created threads.
    /// </summary>
    [JsonPropertyName("default_auto_archive_duration")]
    public int? DefaultAutoArchiveDuration { get; set; }

    /// <summary>
    /// Gets or sets the computed permissions for the invoking user in the channel.
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonConverter(typeof(PermissionStringNullableConverter))]
    public string? Permissions { get; set; }

    /// <summary>
    /// Gets or sets the channel flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public ChannelFlags? Flags { get; set; }

    /// <summary>
    /// Gets or sets the total number of messages sent in a thread.
    /// </summary>
    [JsonPropertyName("total_message_sent")]
    public int? TotalMessageSent { get; set; }
}

/// <summary>
/// Represents a permission overwrite for a channel.
/// </summary>
public class PermissionOverwrite
{
    /// <summary>
    /// Gets or sets the role or user ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of overwrite (0 for role, 1 for member).
    /// </summary>
    [JsonPropertyName("type")]
    public PermissionOverwriteType Type { get; set; }

    /// <summary>
    /// Gets or sets the permission bit set.
    /// </summary>
    [JsonPropertyName("allow")]
    [JsonConverter(typeof(PermissionStringConverter))]
    public string Allow { get; set; } = default!;

    /// <summary>
    /// Gets or sets the permission bit set.
    /// </summary>
    [JsonPropertyName("deny")]
    [JsonConverter(typeof(PermissionStringConverter))]
    public string Deny { get; set; } = default!;
}

/// <summary>
/// Represents thread metadata.
/// </summary>
public class ThreadMetadata
{
    /// <summary>
    /// Gets or sets a value indicating whether the thread is archived.
    /// </summary>
    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    /// <summary>
    /// Gets or sets the duration in minutes to automatically archive the thread after recent activity.
    /// </summary>
    [JsonPropertyName("auto_archive_duration")]
    public int AutoArchiveDuration { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the thread's archive status was last changed.
    /// </summary>
    [JsonPropertyName("archive_timestamp")]
    public string ArchiveTimestamp { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the thread is locked.
    /// </summary>
    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether non-moderators can add other non-moderators to a thread.
    /// </summary>
    [JsonPropertyName("invitable")]
    public bool? Invitable { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the thread was created.
    /// </summary>
    [JsonPropertyName("create_timestamp")]
    public string? CreateTimestamp { get; set; }
}

/// <summary>
/// Represents a thread member.
/// </summary>
public class ThreadMember
{
    /// <summary>
    /// Gets or sets the ID of the thread.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the time the user last joined the thread.
    /// </summary>
    [JsonPropertyName("join_timestamp")]
    public string JoinTimestamp { get; set; } = default!;

    /// <summary>
    /// Gets or sets any user-thread settings.
    /// </summary>
    [JsonPropertyName("flags")]
    public ThreadMemberFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets additional information about the user.
    /// </summary>
    [JsonPropertyName("member")]
    public GuildMember? Member { get; set; }
}

