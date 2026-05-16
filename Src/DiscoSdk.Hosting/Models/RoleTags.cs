using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IRoleTags"/>
internal class RoleTags : IRoleTags
{
    [JsonPropertyName("bot_id")]
    public Snowflake? BotId { get; init; }

    [JsonPropertyName("integration_id")]
    public Snowflake? IntegrationId { get; init; }

    [JsonPropertyName("premium_subscriber")]
    public object? PremiumSubscriber { get; init; }

    [JsonPropertyName("subscription_listing_id")]
    public Snowflake? SubscriptionListingId { get; init; }

    [JsonPropertyName("available_for_purchase")]
    public object? AvailableForPurchase { get; init; }

    [JsonPropertyName("guild_connections")]
    public object? GuildConnections { get; init; }
}
