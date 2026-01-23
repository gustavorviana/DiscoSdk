using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord channel.
/// </summary>
public class Channel
{
    /// <summary>
    /// Gets or sets the channel's unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of channel.
    /// </summary>
    [JsonPropertyName("type")]
    public ChannelType Type { get; set; }

    /// <summary>
    /// Gets or sets the ID of the guild this channel belongs to.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the sorting position of the channel.
    /// </summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }

    /// <summary>
    /// Gets or sets the explicit permission overwrites for members and roles.
    /// </summary>
    [JsonPropertyName("permission_overwrites")]
    public PermissionOverwrite[]? PermissionOverwrites { get; set; }

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
    public Snowflake? LastMessageId { get; set; }

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
    public User[]? Recipients { get; set; }

    /// <summary>
    /// Gets or sets the icon hash of the group DM.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the ID of the creator of the group DM or thread.
    /// </summary>
    [JsonPropertyName("owner_id")]
    public Snowflake? OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the application ID of the group DM creator if it is bot-created.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Snowflake? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the parent channel for the channel.
    /// </summary>
    [JsonPropertyName("parent_id")]
    public Snowflake? ParentId { get; set; }

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
    /// Gets or sets the default duration for newly created threads.
    /// </summary>
    [JsonPropertyName("default_auto_archive_duration")]
    public int? DefaultAutoArchiveDuration { get; set; }

    /// <summary>
    /// Gets or sets the default forum layout view used to display posts in GUILD_FORUM channels.
    /// Defaults to 0, which indicates a layout view has not been set by a channel admin.
    /// </summary>
    [JsonPropertyName("default_forum_layout")]
    public int? DefaultForumLayout { get; set; }

    /// <summary>
    /// Gets or sets the computed permissions for the invoking user in the channel.
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonConverter(typeof(DiscordPermissionConverter))]
    public DiscordPermission Permissions { get; set; }

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

    /// <summary>
    /// Gets or sets the default reaction emoji shown on the add reaction button on forum posts.
    /// </summary>
    [JsonPropertyName("default_reaction_emoji")]
    public DefaultReaction? DefaultReactionEmoji { get; set; }

    /// <summary>
    /// Gets or sets the default sort order type used to order posts in forum channels.
    /// </summary>
    [JsonPropertyName("default_sort_order")]
    public int? DefaultSortOrder { get; set; }

	/// <summary>
	/// Gets or sets the default rate limit per user for threads in this forum channel.
	/// </summary>
	[JsonPropertyName("default_thread_rate_limit_per_user")]
	public int? DefaultThreadRateLimitPerUser { get; set; }

	/// <summary>
	/// Gets or sets the thread metadata for thread channels.
	/// </summary>
	[JsonPropertyName("thread_metadata")]
	public ThreadMetadata? ThreadMetadata { get; set; }

	/// <summary>
	/// Gets or sets the IDs of tags applied to a thread (for threads in forum/media channels).
	/// </summary>
	[JsonPropertyName("applied_tags")]
	public Snowflake[]? AppliedTags { get; set; }

	/// <summary>
	/// Gets or sets the set of tags that can be used in a forum or media channel.
	/// </summary>
	[JsonPropertyName("available_tags")]
	public ForumTag[]? AvailableTags { get; set; }

	/// <summary>
	/// Gets or sets the version of the channel.
	/// </summary>
	[JsonPropertyName("version")]
	public long Version { get; set; }
}
