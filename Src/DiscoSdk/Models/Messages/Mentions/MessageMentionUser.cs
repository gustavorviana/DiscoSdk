using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Mentions;

/// <summary>
/// Represents a user mentioned in a Discord message.
///
/// This object is materialized from the <c>message.mentions</c> array returned by
/// the Discord API and corresponds to a global <c>User</c>, not a guild-specific
/// <c>Member</c>.
///
/// It provides all information necessary to identify and render the mentioned user
/// without requiring additional API calls, including:
/// - The user identity (<see cref="UserId"/>, <see cref="Username"/>, <see cref="GlobalName"/>)
/// - Visual data (<see cref="Avatar"/>, <see cref="Banner"/>, <see cref="AccentColor"/>)
/// - Account metadata (<see cref="PublicFlags"/>, <see cref="Flags"/>)
/// - Extended profile data (<see cref="Clan"/>, <see cref="PrimaryGuild"/>)
///
/// This type exists to bridge the gap between the raw textual mention in the message
/// (e.g. <c>&lt;@123&gt;</c>) and a strongly-typed domain model, allowing consumers
/// to iterate over mentioned users, inspect their profile data, and render UI
/// without performing extra REST requests.
///
/// Note that this is always a <c>User</c>-level view. In guild contexts, member-
/// specific data (nickname, roles, join date, etc.) is not guaranteed to be present
/// and must be resolved separately if required.
/// </summary>
public class MessageMentionUser
{
    /// <summary>
    /// The user's unique Discord ID (snowflake).
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake UserId { get; set; }

    /// <summary>
    /// The user's account username (handle). This is not the guild nickname.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// The hash of the user's avatar image.
    /// Use it to construct the CDN URL for the avatar.
    /// Null indicates that the user has no custom avatar.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// The user's discriminator.
    /// For modern Discord accounts, this is typically "0".
    /// </summary>
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = null!;

    /// <summary>
    /// Bitfield representing public account badges and attributes.
    /// </summary>
    [JsonPropertyName("public_flags")]
    public UserFlags PublicFlags { get; set; } = UserFlags.None;

    /// <summary>
    /// Bitfield representing internal or contextual user flags.
    /// Depending on the endpoint, this may mirror <see cref="PublicFlags"/>.
    /// </summary>
    [JsonPropertyName("flags")]
    public UserFlags Flags { get; set; } = UserFlags.None;

    /// <summary>
    /// The hash of the user's profile banner image.
    /// Use it to construct the CDN URL for the banner.
    /// Null indicates that no banner is set.
    /// </summary>
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    /// <summary>
    /// The user's chosen accent color for their profile, expressed as an RGB integer.
    /// Null when not set.
    /// </summary>
    [JsonPropertyName("accent_color")]
    public Color? AccentColor { get; set; }

    /// <summary>
    /// The user's global display name.
    /// This is the name shown across Discord, independent of guilds.
    /// </summary>
    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    /// <summary>
    /// Avatar decoration metadata.
    /// This field is experimental and may change shape or be null.
    /// </summary>
    [JsonPropertyName("avatar_decoration_data")]
    public AvatarDecorationData? AvatarDecorationData { get; set; }

    /// <summary>
    /// Collectibles metadata.
    /// This field is experimental and may change shape or be null.
    /// </summary>
    [JsonPropertyName("collectibles")]
    public UserCollectibles? Collectibles { get; set; }

    /// <summary>
    /// Display-name styling metadata.
    /// This field is experimental and may change shape or be null.
    /// </summary>
    [JsonPropertyName("display_name_styles")]
    public DisplayNameStyles? DisplayNameStyles { get; set; }

    /// <summary>
    /// The user's banner color expressed as a hex string (e.g. "#b84141").
    /// Null when not set.
    /// </summary>
    [JsonPropertyName("banner_color")]
    public Color? BannerColor { get; set; }

    /// <summary>
    /// Clan identity information associated with the user.
    /// This is feature-specific and may be absent.
    /// </summary>
    [JsonPropertyName("clan")]
    public UserClan? Clan { get; set; }

    /// <summary>
    /// Primary guild identity information associated with the user.
    /// This is feature-specific and may be absent.
    /// </summary>
    [JsonPropertyName("primary_guild")]
    public PrimaryGuild? PrimaryGuild { get; set; }

    public DiscordImageUrl? GetAvatarImage()
    {
        return DiscordImageUrl.ParseAvatar(UserId, Avatar);
    }

    public DiscordImageUrl? GetBannerImage()
    {
        return DiscordImageUrl.ParseBanner(UserId, Banner);
    }
}