# SPEC-SDK-05 — Observability

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-SDK-05](../prd/sdk-05-observability.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — |
| **Last updated** | 2026-09-29 |

## 1. Summary
`DiscoSdkDiagnostics` (public static class) exposes the names `MeterName` and `ActivitySourceName`, both
`DiscoSdk`. It also holds internal static instruments on one `Meter` and one `ActivitySource`, versioned
with the assembly version. The producers record into these instruments directly:
- `Shard`;
- `ShardEventDispatcher`;
- `DiscordClient`;
- `DiscordEventDispatcher`;
- `BucketRequestQueue`;
- `MemberManager` and `StickerManager`.

`DiagnosticTags` centralises the tag keys and the constant values. `TokenSanitizer` (in `DiscoSdk`) masks
secrets for display.

## 2. Projects and dependencies
- `DiscoSdk.Hosting/Observability/DiscoSdkDiagnostics.cs`, `DiagnosticTags.cs`.
- `DiscoSdk/TokenSanitizer.cs`.
- Packages: none (`System.Diagnostics.DiagnosticSource` is part of the base class library in .NET 8).

## 3. Models and contracts
**Instruments**

| Instrument | Kind / unit | Tags | Producer |
|---|---|---|---|
| `discosdk.gateway.events_received` | Counter `{event}` | shard, event type | `DiscordClient` |
| `discosdk.gateway.heartbeat.latency` | Histogram ms | shard | `Shard` |
| `discosdk.gateway.event.dispatch_duration` | Histogram ms | shard, event type | `ShardEventDispatcher` |
| `discosdk.gateway.lifecycle` | Counter `{transition}` | shard, phase (connect / identify / resume / ready / disconnect / invalidate) | `Shard` |
| `discosdk.gateway.reconnects` | Counter `{attempt}` | shard | `Shard` |
| `discosdk.rest.requests` | Counter `{request}` | route, method, status class | `BucketRequestQueue` |
| `discosdk.rest.latency` | Histogram ms | route, method | `BucketRequestQueue` |
| `discosdk.rest.rate_limited` | Counter `{response}` | route, scope (bucket / shared / global) | `BucketRequestQueue` |
| `discosdk.cache.lookups` | Counter `{lookup}` | entity, result (hit / miss / rest) | `MemberManager`, `StickerManager` |
| `discosdk.cache.evictions` | Counter `{entry}` | entity | none (never recorded) |
| `discosdk.handler.invocations` | Counter `{invocation}` | handler type, event type, outcome, exception type | `DiscordEventDispatcher` |
| `discosdk.handler.latency` | Histogram ms | handler type, event type | `DiscordEventDispatcher` |

**Spans**
- `discosdk.gateway.dispatch` (Consumer), tagged with the event type.
- `discosdk.handler.invoke` (Internal), tagged with the handler type, the event type and
  `discosdk.handler.fire_and_forget`. It is a child of the dispatch span; `Task.Run` flows
  `Activity.Current`, so this also holds for fire-and-forget handlers.
- `discosdk.rest.request` (Client), tagged with the route (the bucket key).

## 4. Components
| Type | Responsibility |
|---|---|
| `DiscoSdkDiagnostics` | Names, `Meter`, `ActivitySource` and instrument definitions. |
| `DiagnosticTags` | Tag keys (`discord.shard.id`, `discord.event.type`, `discord.route`, `discord.scope`, `http.method`, `http.status_class`, `discosdk.cache.*`, `discosdk.handler.*`, `exception.type`, `discord.gateway.phase`) and `ClassifyStatus`. |
| `TokenSanitizer` | `Mask` keeps the last 4 characters and replaces the rest with at most 12 `*`. `MaskAll` masks every `Bot ` / `Bearer ` value in a text. |

## 5. Public API
`DiscoSdkDiagnostics.MeterName`, `DiscoSdkDiagnostics.ActivitySourceName`, the `DiagnosticTags` constants,
`TokenSanitizer.Mask`, `TokenSanitizer.MaskAll`.

## 6. Discord surface
None. The REST tags reflect the `X-RateLimit-Scope` header (SPEC-API-002).

## 7. Flows
**REST request**
1. `BucketRequestQueue` starts `discosdk.rest.request`.
2. It sends the request and measures the elapsed time.
3. It records `RestRequests` (status class) and `RestLatency`.
4. On a 429, it records `RestRateLimited` with the normalised scope.

**Gateway event**
1. `DiscordClient` counts the event.
2. `ShardEventDispatcher` measures the dispatch duration.
3. `DiscordEventDispatcher` starts the dispatch span and one handler span per handler, and records the
   handler metrics.

## 8. Concurrency and lifecycle
The instruments are static and thread-safe. The meter and source live for the process lifetime and are
shared across `DiscordClient` instances in the same process, with no client or instance tag.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| No listener | Instruments and `StartActivity` return immediately or `null` (OB-N01). |
| Several clients in one process | Metrics are merged. **Proposed:** an optional `discosdk.client` tag (application id). |
| A route tag containing ids | **Current:** `METHOD /channels/123…`, then `<hash>:/channels/123…` once the bucket hash is known (`DiscordRestClient.OnBucketHashLearned`). **Proposed:** `DiscordRoute.Template`. |
| An exception message containing a token | Not masked. `MaskAll` is not wired into logging. |

## 10. Observability
This document is the observability contract.

## 11. Tests
| Test class | Covers |
|---|---|
| `DiscoSdkDiagnosticsTests` | OB-F01, OB-F07 (`ClassifyStatus`) |
| `GatewayEventsReceivedMetricTests`, `ShardHeartbeatLatencyMetricTests` | OB-F02 |
| `BucketRequestQueueMetricsTests` | OB-F03 |
| `HandlerInvocationsMetricTests` | OB-F04 |
| `CacheLookupsMetricTests` | OB-F05 |

No dedicated tests cover OB-F06 (spans), OB-F08, OB-N01, OB-N02 or OB-N03.

## 12. History
| Commit | Change |
|---|---|
| `b9ff1b6` | Cache, handler and lifecycle instrumentation, and spans. |

Next steps:
1. Use the route template in the `discord.route` tag.
2. Record `CacheEvictions` when a member is evicted by policy.
3. Wire `TokenSanitizer.MaskAll` into REST exception messages and gateway close logs.
4. Add a command-name tag to interaction handler spans.
5. Add an optional health check package.

## 13. Decisions and rejected alternatives
- **BCL `Meter` / `ActivitySource` instead of an OpenTelemetry dependency**: zero dependencies, and any
  listener works (OpenTelemetry, `dotnet-counters`, a custom `MeterListener`).
- **Static instruments**: the simplest producer path. The cost is no per-client separation, which is
  acceptable because one process usually hosts one bot.
