using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord role.
/// </summary>
public class Role
{
    /// <summary>
    /// Gets or sets the role's unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the role's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the role's color.
    /// </summary>
    [JsonPropertyName("color")]
    public int Color { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role is hoisted.
    /// </summary>
    [JsonPropertyName("hoist")]
    public bool Hoist { get; set; }

    /// <summary>
    /// Gets or sets the role's icon hash.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the role's unicode emoji.
    /// </summary>
    [JsonPropertyName("unicode_emoji")]
    public string? UnicodeEmoji { get; set; }

    /// <summary>
    /// Gets or sets the role's position.
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the role's permissions as a string (bitfield).
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonConverter(typeof(SafeStringConverter))]
    public string Permissions { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the role is managed by an integration.
    /// </summary>
    [JsonPropertyName("managed")]
    public bool Managed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role is mentionable.
    /// </summary>
    [JsonPropertyName("mentionable")]
    public bool Mentionable { get; set; }

    /// <summary>
    /// Gets or sets the tags for the role.
    /// </summary>
    [JsonPropertyName("tags")]
    public RoleTags? Tags { get; set; }

    /// <summary>
    /// Gets or sets the role flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public RoleFlags? Flags { get; set; }
}

/// <summary>
/// Represents tags associated with a role.
/// </summary>
public class RoleTags
{
    /// <summary>
    /// Gets or sets the bot ID this role belongs to.
    /// </summary>
    [JsonPropertyName("bot_id")]
    public string? BotId { get; set; }

    /// <summary>
    /// Gets or sets the integration ID this role belongs to.
    /// </summary>
    [JsonPropertyName("integration_id")]
    public string? IntegrationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role this role is the premium subscriber role for.
    /// </summary>
    [JsonPropertyName("premium_subscriber")]
    public object? PremiumSubscriber { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role this role is linked to.
    /// </summary>
    [JsonPropertyName("subscription_listing_id")]
    public string? SubscriptionListingId { get; set; }

    /// <summary>
    /// Gets or sets whether this is the guild's Booster role.
    /// </summary>
    [JsonPropertyName("available_for_purchase")]
    public object? AvailableForPurchase { get; set; }

    /// <summary>
    /// Gets or sets the ID of the guild this role is linked to.
    /// </summary>
    [JsonPropertyName("guild_connections")]
    public object? GuildConnections { get; set; }
}

