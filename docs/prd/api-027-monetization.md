# PRD-API-027 — Monetization (SKUs, entitlements, subscriptions)

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-027](../specs/api-027-monetization.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/sku](https://docs.discord.com/developers/resources/sku), [resources/entitlement](https://docs.discord.com/developers/resources/entitlement), [resources/subscription](https://docs.discord.com/developers/resources/subscription) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Premium apps sell SKUs (durable, consumable, subscriptions). Bots check entitlements to unlock features, consume one-time purchases, list a user's subscriptions, and create test entitlements during development. Premium buttons (component style 6) replace the deprecated `PREMIUM_REQUIRED` callback.

## 2. Goals
- List SKUs, and query or consume entitlements, with every filter.
- Read subscriptions.
- Create and delete test entitlements.

## 3. Out of scope
- Premium button: [PRD-API-010](api-010-components.md). Entitlement and subscription gateway events: [PRD-API-004](api-004-gateway-events.md). Entitlements in interactions (`entitlements` field): [PRD-API-009](api-009-interactions.md).

## 4. Usage scenarios
- As a bot author, before running `/pro-feature` I check that the user has an active entitlement.
- After a consumable is used, I consume the entitlement.

## 5. Desired developer experience

```csharp
var entitlements = await client.Monetization.GetEntitlements(userId: user.Id, excludeEnded: true).ExecuteAsync();
if (entitlements.Count == 0) { /* offer the premium button */ }
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| MN-F01 | List Entitlements | `GET /applications/{}/entitlements` | Must | Implemented | `IMonetization.GetEntitlements()` → `ApplicationClient.ListEntitlementsAsync` |
| MN-F02 | Get Entitlement | `GET /applications/{}/entitlements/{}` | Must | Implemented | `IMonetization.GetEntitlement()` → `ApplicationClient.GetEntitlementAsync` |
| MN-F03 | Consume an Entitlement | `POST /applications/{}/entitlements/{}/consume` | Must | Implemented | `IEntitlement.Consume()`, `IMonetization.ConsumeEntitlement()` → `ApplicationClient.ConsumeEntitlementAsync` |
| MN-F04 | Create Test Entitlement | `POST /applications/{}/entitlements` | Should | Partial | `ApplicationClient.CreateTestEntitlementAsync` exists, but no public API reaches it |
| MN-F05 | Delete Test Entitlement | `DELETE /applications/{}/entitlements/{}` | Must | Implemented | `IEntitlement.DeleteTest()` → `ApplicationClient.DeleteTestEntitlementAsync` |
| MN-F06 | List SKUs | `GET /applications/{}/skus` | Must | Implemented | `IMonetization.GetSkus()` → `ApplicationClient.ListSkusAsync` |
| MN-F07 | List SKU Subscriptions | `GET /skus/{}/subscriptions` | Must | Implemented | `IMonetization.GetSkuSubscriptions()`, `ISku.GetSubscriptions()` → `ApplicationClient.ListSkuSubscriptionsAsync` |
| MN-F08 | Get SKU Subscription | `GET /skus/{}/subscriptions/{}` | Must | Implemented | `IMonetization.GetSkuSubscription()`, `ISku.GetSubscription()` → `ApplicationClient.GetSkuSubscriptionAsync` |
| MN-F09 | SKU object (type, flags, name, slug) | — | Must | Implemented | `ISku`, `SkuType`, `SkuFlags` |
| MN-F10 | Entitlement object (type, user, guild, starts/ends, deleted, consumed) | — | Must | Implemented | `IEntitlement`, `EntitlementType` |
| MN-F11 | Subscription object (status, SKUs, entitlements, renewal, current period) | — | Should | Implemented | `ISubscription`, `SubscriptionStatus` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| MN-N01 | Entitlement queries support pagination parameters (`before`, `after`, `limit`) and filters (`user_id`, `sku_ids`, `guild_id`, `exclude_ended`, `exclude_deleted`). |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] SKUs, entitlements and subscriptions (`ApplicationClientTests`, `SkuWrapperTests`, `EntitlementWrapperTests`).
- [x] Entitlement and subscription events (`EntitlementDispatchTests`, `SubscriptionDispatchTests`).
- [ ] Test entitlements can be created from the public API (MN-F04).

## 11. Open questions
- None.
