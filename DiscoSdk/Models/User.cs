using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord user.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the user's unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public DiscordId Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's username.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's discriminator.
    /// </summary>
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's global display name.
    /// </summary>
    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    /// <summary>
    /// Gets or sets the user's avatar hash.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is a bot.
    /// </summary>
    [JsonPropertyName("bot")]
    public bool? Bot { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is a system user.
    /// </summary>
    [JsonPropertyName("system")]
    public bool? System { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has two-factor authentication enabled.
    /// </summary>
    [JsonPropertyName("mfa_enabled")]
    public bool? MfaEnabled { get; set; }

    /// <summary>
    /// Gets or sets the user's banner hash.
    /// </summary>
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    /// <summary>
    /// Gets or sets the user's accent color.
    /// </summary>
    [JsonPropertyName("accent_color")]
    public int? AccentColor { get; set; }

    /// <summary>
    /// Gets or sets the user's locale.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's email is verified.
    /// </summary>
    [JsonPropertyName("verified")]
    public bool? Verified { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the user's flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public UserFlags? Flags { get; set; }

    /// <summary>
    /// Gets or sets the user's premium type.
    /// </summary>
    [JsonPropertyName("premium_type")]
    public PremiumType? PremiumType { get; set; }

    /// <summary>
    /// Gets or sets the user's public flags.
    /// </summary>
    [JsonPropertyName("public_flags")]
    public UserFlags? PublicFlags { get; set; }
}