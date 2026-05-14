using DiscoSdk.Models;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Public surface for SKU / entitlement / subscription operations. Exposed via
/// <see cref="IDiscordClient.Monetization"/>.
/// </summary>
public interface IMonetization
{
    /// <summary>Lists the SKUs (premium offerings) for this application.</summary>
    IRestAction<IReadOnlyList<ISku>> GetSkus();

    /// <summary>Lists entitlements for this application, optionally filtered.</summary>
    /// <param name="userId">If set, only entitlements granted to this user.</param>
    /// <param name="guildId">If set, only entitlements granted to this guild.</param>
    /// <param name="excludeEnded">If <c>true</c>, exclude entitlements whose period has ended.</param>
    /// <param name="excludeDeleted">If <c>false</c>, include deleted entitlements (default is to exclude them).</param>
    IRestAction<IReadOnlyList<IEntitlement>> GetEntitlements(Snowflake? userId = null, Snowflake? guildId = null, bool? excludeEnded = null, bool? excludeDeleted = null);

    /// <summary>Retrieves a single entitlement by ID.</summary>
    IRestAction<IEntitlement> GetEntitlement(Snowflake entitlementId);

    /// <summary>Marks a one-time-purchase consumable entitlement as consumed.</summary>
    IRestAction ConsumeEntitlement(Snowflake entitlementId);

    /// <summary>Lists subscriptions for a SKU.</summary>
    /// <param name="skuId">The SKU to list subscriptions for.</param>
    /// <param name="userId">If set, only the subscription owned by this user.</param>
    IRestAction<IReadOnlyList<ISubscription>> GetSkuSubscriptions(Snowflake skuId, Snowflake? userId = null);

    /// <summary>Retrieves a single SKU subscription by ID.</summary>
    IRestAction<ISubscription> GetSkuSubscription(Snowflake skuId, Snowflake subscriptionId);
}
