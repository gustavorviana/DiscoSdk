using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents tags associated with a role.
/// </summary>
public class RoleTags
{
    /// <summary>
    /// Gets or sets the bot ID this role belongs to.
    /// </summary>
    [JsonPropertyName("bot_id")]
    public Snowflake? BotId { get; set; }

    /// <summary>
    /// Gets or sets the integration ID this role belongs to.
    /// </summary>
    [JsonPropertyName("integration_id")]
    public Snowflake? IntegrationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role this role is the premium subscriber role for.
    /// </summary>
    [JsonPropertyName("premium_subscriber")]
    public object? PremiumSubscriber { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role this role is linked to.
    /// </summary>
    [JsonPropertyName("subscription_listing_id")]
    public Snowflake? SubscriptionListingId { get; set; }

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