# SPEC-API-027 — Monetization (SKUs, entitlements, subscriptions)

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-027](../prd/api-027-monetization.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/sku](https://docs.discord.com/developers/resources/sku), [resources/entitlement](https://docs.discord.com/developers/resources/entitlement), [resources/subscription](https://docs.discord.com/developers/resources/subscription) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IDiscordClient.Monetization` (`MonetizationSurface`) exposes SKUs, entitlement queries, consume and subscription lookups. `EntitlementWrapper` (`IEntitlement`) provides `Consume()` and `DeleteTest()`, and `SkuWrapper` (`ISku`) provides the subscription helpers. All routes are on `ApplicationClient`.

## 2. Projects and dependencies
`DiscoSdk` (`IMonetization`, `ISku`, `IEntitlement`, `ISubscription`, `SkuType`, `SkuFlags`, `EntitlementType`, `SubscriptionStatus`), and `DiscoSdk.Hosting` (`ApplicationClient`, `MonetizationSurface`, the wrappers).

## 3. Models and contracts
Entitlement, SKU and subscription wire models map the documented fields. The subscription period fields are raw ISO strings.

## 4. Components
| Type | Responsibility |
|---|---|
| `ApplicationClient` | `ListSkusAsync`, `ListEntitlementsAsync(filters)`, `GetEntitlementAsync`, `ConsumeEntitlementAsync`, `CreateTestEntitlementAsync` (not exposed), `DeleteTestEntitlementAsync`, `ListSkuSubscriptionsAsync`, `GetSkuSubscriptionAsync`. |

## 5. Public API
`IDiscordClient.Monetization.GetSkus/GetEntitlements/GetEntitlement/ConsumeEntitlement/GetSkuSubscriptions/GetSkuSubscription`, `IEntitlement.Consume()` / `DeleteTest()`, `ISku.GetSubscriptions()` / `GetSubscription()`.

## 6. Discord surface
Eight routes across three pages. Entitlement and subscription events are covered in PRD-API-004.

## 7. Flows
Check access: `GetEntitlements(userId, excludeEnded: true)` → a non-empty list means access.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Consuming a non-consumable entitlement | Discord 400. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `ApplicationClientTests` | MN-F01 – MN-F08, MN-N01 |
| `SkuWrapperTests`, `EntitlementWrapperTests` | MN-F09 – MN-F11 |
| `EntitlementDispatchTests`, `SubscriptionDispatchTests` | events (PRD-API-004) |

## 12. History
| Commit | Change |
|---|---|
| `225dad3` | Cohesive surfaces (`IMonetization`). |
| `9f81c56` | Interface-first models. |

Next steps: expose `CreateTestEntitlement`, and type the subscription period fields as `DateTimeOffset`.

## 13. Decisions and rejected alternatives
- **One `IMonetization` surface** rather than three: SKUs, entitlements and subscriptions are always used together.
