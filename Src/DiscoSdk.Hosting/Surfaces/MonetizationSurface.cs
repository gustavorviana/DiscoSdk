using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IMonetization"/>. Delegates to <see cref="ApplicationClient"/>.
/// </summary>
internal sealed class MonetizationSurface(DiscordClient client) : IMonetization
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<ISku>> GetSkus()
        => RestAction<IReadOnlyList<ISku>>.Create(async ct =>
        {
            var skus = await _client.ApplicationClient.ListSkusAsync(_client.RequireApplicationId(), ct);
            return skus.Select(s => (ISku)new SkuWrapper(_client, s)).ToList().AsReadOnly();
        });

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<IEntitlement>> GetEntitlements(Snowflake? userId = null, Snowflake? guildId = null, bool? excludeEnded = null, bool? excludeDeleted = null)
        => RestAction<IReadOnlyList<IEntitlement>>.Create(async ct =>
        {
            var entitlements = await _client.ApplicationClient.ListEntitlementsAsync(_client.RequireApplicationId(), userId, guildId, excludeEnded, excludeDeleted, ct);
            return entitlements.Select(e => (IEntitlement)new EntitlementWrapper(_client, e)).ToList().AsReadOnly();
        });

    /// <inheritdoc />
    public IRestAction<IEntitlement> GetEntitlement(Snowflake entitlementId)
        => RestAction<IEntitlement>.Create(async ct =>
            new EntitlementWrapper(_client, await _client.ApplicationClient.GetEntitlementAsync(_client.RequireApplicationId(), entitlementId, ct)));

    /// <inheritdoc />
    public IRestAction ConsumeEntitlement(Snowflake entitlementId)
        => RestAction.Create(ct => _client.ApplicationClient.ConsumeEntitlementAsync(_client.RequireApplicationId(), entitlementId, ct));

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<ISubscription>> GetSkuSubscriptions(Snowflake skuId, Snowflake? userId = null)
        => RestAction<IReadOnlyList<ISubscription>>.Create(async ct => await _client.ApplicationClient.ListSkuSubscriptionsAsync(skuId, userId, ct));

    /// <inheritdoc />
    public IRestAction<ISubscription> GetSkuSubscription(Snowflake skuId, Snowflake subscriptionId)
        => RestAction<ISubscription>.Create(async ct => await _client.ApplicationClient.GetSkuSubscriptionAsync(skuId, subscriptionId, ct));
}
