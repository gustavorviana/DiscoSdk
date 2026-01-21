using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Mentions;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Users;

/// <summary>
/// Represents a Discord user.
/// </summary>
public class User : MessageMentionUser
{
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
    /// Gets or sets the user's premium type.
    /// </summary>
    [JsonPropertyName("premium_type")]
    public PremiumType PremiumType { get; set; } = PremiumType.None;

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}