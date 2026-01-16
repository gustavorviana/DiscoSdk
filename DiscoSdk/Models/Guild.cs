using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord guild (server).
/// </summary>
public class Guild
{
    /// <summary>
    /// Gets or sets the guild's unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the guild's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the guild's icon hash.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the guild's icon hash, returned when in the template object.
    /// </summary>
    [JsonPropertyName("icon_hash")]
    public string? IconHash { get; set; }

    /// <summary>
    /// Gets or sets the guild's splash hash.
    /// </summary>
    [JsonPropertyName("splash")]
    public string? Splash { get; set; }

    /// <summary>
    /// Gets or sets the guild's discovery splash hash.
    /// </summary>
    [JsonPropertyName("discovery_splash")]
    public string? DiscoverySplash { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user is the owner of the guild.
    /// </summary>
    [JsonPropertyName("owner")]
    public bool? Owner { get; set; }

    /// <summary>
    /// Gets or sets the ID of the guild owner.
    /// </summary>
    [JsonPropertyName("owner_id")]
    public Snowflake? OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the permissions for the current user in the guild.
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonConverter(typeof(PermissionIntegerNullableConverter))]
    public int? Permissions { get; set; }

    /// <summary>
    /// Gets or sets the guild's region.
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// Gets or sets the ID of the AFK channel.
    /// </summary>
    [JsonPropertyName("afk_channel_id")]
    public Snowflake? AfkChannelId { get; set; }

    /// <summary>
    /// Gets or sets the AFK timeout in seconds.
    /// </summary>
    [JsonPropertyName("afk_timeout")]
    public int? AfkTimeout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the guild widget is enabled.
    /// </summary>
    [JsonPropertyName("widget_enabled")]
    public bool? WidgetEnabled { get; set; }

    /// <summary>
    /// Gets or sets the channel ID for the guild widget.
    /// </summary>
    [JsonPropertyName("widget_channel_id")]
    public Snowflake? WidgetChannelId { get; set; }

    /// <summary>
    /// Gets or sets the verification level required for the guild.
    /// </summary>
    [JsonPropertyName("verification_level")]
    public VerificationLevel? VerificationLevel { get; set; }

    /// <summary>
    /// Gets or sets the default message notification level.
    /// </summary>
    [JsonPropertyName("default_message_notifications")]
    public DefaultMessageNotificationLevel? DefaultMessageNotifications { get; set; }

    /// <summary>
    /// Gets or sets the explicit content filter level.
    /// </summary>
    [JsonPropertyName("explicit_content_filter")]
    public ExplicitContentFilterLevel? ExplicitContentFilter { get; set; }

    /// <summary>
    /// Gets or sets the roles in the guild.
    /// </summary>
    [JsonPropertyName("roles")]
    public Role[]? Roles { get; set; }

    /// <summary>
    /// Gets or sets the emojis in the guild.
    /// </summary>
    [JsonPropertyName("emojis")]
    public Emoji[]? Emojis { get; set; }

    /// <summary>
    /// Gets or sets the guild's features.
    /// </summary>
    [JsonPropertyName("features")]
    public string[]? Features { get; set; }

    /// <summary>
    /// Gets or sets the required MFA level for the guild.
    /// </summary>
    [JsonPropertyName("mfa_level")]
    public MfaLevel? MfaLevel { get; set; }

    /// <summary>
    /// Gets or sets the application ID of the guild creator if it is bot-created.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Snowflake? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the system channel ID.
    /// </summary>
    [JsonPropertyName("system_channel_id")]
    public Snowflake? SystemChannelId { get; set; }

    /// <summary>
    /// Gets or sets the system channel flags.
    /// </summary>
    [JsonPropertyName("system_channel_flags")]
    public SystemChannelFlags? SystemChannelFlags { get; set; }

    /// <summary>
    /// Gets or sets the rules channel ID.
    /// </summary>
    [JsonPropertyName("rules_channel_id")]
    public Snowflake? RulesChannelId { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of presences for the guild.
    /// </summary>
    [JsonPropertyName("max_presences")]
    public int? MaxPresences { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of members for the guild.
    /// </summary>
    [JsonPropertyName("max_members")]
    public int? MaxMembers { get; set; }

    /// <summary>
    /// Gets or sets the vanity URL code for the guild.
    /// </summary>
    [JsonPropertyName("vanity_url_code")]
    public string? VanityUrlCode { get; set; }

    /// <summary>
    /// Gets or sets the description of the guild.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the banner hash of the guild.
    /// </summary>
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    /// <summary>
    /// Gets or sets the premium tier.
    /// </summary>
    [JsonPropertyName("premium_tier")]
    public PremiumTier? PremiumTier { get; set; }

    /// <summary>
    /// Gets or sets the number of boosters this guild currently has.
    /// </summary>
    [JsonPropertyName("premium_subscription_count")]
    public int? PremiumSubscriptionCount { get; set; }

    /// <summary>
    /// Gets or sets the preferred locale of a guild.
    /// </summary>
    [JsonPropertyName("preferred_locale")]
    public string? PreferredLocale { get; set; }

    /// <summary>
    /// Gets or sets the ID of the channel where guild notices are posted.
    /// </summary>
    [JsonPropertyName("public_updates_channel_id")]
    public Snowflake? PublicUpdatesChannelId { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of users in a video channel.
    /// </summary>
    [JsonPropertyName("max_video_channel_users")]
    public int? MaxVideoChannelUsers { get; set; }

    /// <summary>
    /// Gets or sets the approximate number of members in the guild.
    /// </summary>
    [JsonPropertyName("approximate_member_count")]
    public int? ApproximateMemberCount { get; set; }

    /// <summary>
    /// Gets or sets the approximate number of online members in the guild.
    /// </summary>
    [JsonPropertyName("approximate_presence_count")]
    public int? ApproximatePresenceCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the guild is unavailable.
    /// </summary>
    [JsonPropertyName("unavailable")]
    public bool? Unavailable { get; set; }

    /// <summary>
    /// Gets or sets all channels received in the GUILD_CREATE event payload.
    /// This includes all channel types: text, voice, thread, forum, media, stage, news, etc.
    /// </summary>
    [JsonPropertyName("channels")]
    public Channel[] Channels { get; set; } = [];
}