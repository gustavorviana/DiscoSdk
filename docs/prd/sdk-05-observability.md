# PRD-SDK-05 — Observability

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-SDK-05](../specs/sdk-05-observability.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — (SDK framework feature) |
| **Last updated** | 2026-09-29 |

## 1. Problem
When a bot misbehaves in production, its operator needs to know whether the cause is the gateway
(reconnect loops, heartbeat latency), REST (rate limits, 5xx), slow handlers, or cache misses. Without
standard metrics and traces this means reading logs by hand. Logs and telemetry must also never leak the
bot token.

## 2. Goals
- Metrics on one `System.Diagnostics.Metrics.Meter` and traces on one `ActivitySource`, both named
  `DiscoSdk`, consumable by OpenTelemetry without an SDK dependency.
- Zero overhead when no listener is attached.
- Bounded-cardinality tags that follow OpenTelemetry conventions where one exists.
- Token masking helpers used wherever configuration or headers can be printed.

## 3. Out of scope
- Shipping an OpenTelemetry exporter or a Grafana dashboard.
- Structured logging design (the SDK uses `ILogger`; see PRD-SDK-01 CB-F03).

## 4. Usage scenarios
- As an operator, I add `AddMeter(DiscoSdkDiagnostics.MeterName)` to OpenTelemetry and chart REST 429s by
  scope.
- I trace one slow interaction from `discosdk.gateway.dispatch` through `discosdk.handler.invoke` to the
  `discosdk.rest.request` it made.
- I alert when `discosdk.gateway.reconnects` spikes for one shard.

## 5. Desired developer experience

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter(DiscoSdkDiagnostics.MeterName))
    .WithTracing(t => t.AddSource(DiscoSdkDiagnostics.ActivitySourceName));
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| OB-F01 | Public meter and activity source names | — | Must | Implemented | `DiscoSdkDiagnostics.MeterName`, `DiscoSdkDiagnostics.ActivitySourceName` |
| OB-F02 | Gateway metrics: events received, heartbeat latency, dispatch duration, lifecycle transitions, reconnects | — | Must | Implemented | `DiscoSdkDiagnostics.GatewayEventsReceived`, `DiscoSdkDiagnostics.GatewayHeartbeatLatency`, `DiscoSdkDiagnostics.GatewayEventDispatchDuration`, `DiscoSdkDiagnostics.GatewayLifecycle`, `DiscoSdkDiagnostics.GatewayReconnects` |
| OB-F03 | REST metrics: requests by status class, latency, 429s by scope | — | Must | Partial | `DiscoSdkDiagnostics.RestRequests`, `DiscoSdkDiagnostics.RestLatency`, `DiscoSdkDiagnostics.RestRateLimited`. The `discord.route` tag carries the bucket key (`METHOD /channels/<id>` or `<hash>:/channels/<id>`), not a route template, so its cardinality grows with every channel, guild and webhook. |
| OB-F04 | Handler metrics: invocations by outcome and exception type, latency | — | Must | Implemented | `DiscoSdkDiagnostics.HandlerInvocations`, `DiscoSdkDiagnostics.HandlerLatency` |
| OB-F05 | Cache metrics: lookups by entity and result, evictions | — | Should | Partial | `DiscoSdkDiagnostics.CacheLookups` (members and stickers); `DiscoSdkDiagnostics.CacheEvictions` is never recorded, and presence lookups are not counted |
| OB-F06 | Spans for gateway dispatch, handler invocation and REST request | — | Must | Implemented | `discosdk.gateway.dispatch` (Consumer), `discosdk.handler.invoke` (Internal), `discosdk.rest.request` (Client) |
| OB-F07 | Central tag keys and values | — | Should | Implemented | `DiagnosticTags` |
| OB-F08 | Token masking for configuration and Authorization-style text | — | Must | Partial | `TokenSanitizer.Mask` is used by `DiscordClientConfig.ToString`; `TokenSanitizer.MaskAll` exists but no log or exception path calls it |
| OB-F09 | Interaction or command span attributes (command name, interaction type) | — | Could | Missing | Handler spans carry only the handler type and event type |
| OB-F10 | Health check (`IHealthCheck`) reporting shard state | — | Could | Missing | — |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| OB-N01 | Instruments and activities are no-ops without listeners: no allocation on the hot path. |
| OB-N02 | Tag cardinality is bounded: status codes are bucketed into classes, and tags hold no ids or user content. **Currently violated by the `discord.route` tag (OB-F03).** |
| OB-N03 | No telemetry, log or exception message contains a bot token, bearer token, webhook token or interaction token. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Meter name | `DiscoSdk` | constant | `DiscoSdkDiagnostics.MeterName` |
| Activity source name | `DiscoSdk` | constant | `DiscoSdkDiagnostics.ActivitySourceName` |

Metrics and traces are enabled by attaching a listener. There is no builder switch.

## 9. Compatibility
Instrument names and tag keys are public contracts that dashboards depend on. Renames require a
deprecation period in which both names are emitted. Changing the `discord.route` value to a template is a
value change, not a key change, but dashboards that filter on the current values will break.

## 10. Acceptance criteria
- [x] Meter and source names (`DiscoSdkDiagnosticsTests`).
- [x] Gateway, REST, handler and cache metrics (`GatewayEventsReceivedMetricTests`, `ShardHeartbeatLatencyMetricTests`, `BucketRequestQueueMetricsTests`, `HandlerInvocationsMetricTests`, `CacheLookupsMetricTests`).
- [ ] `discord.route` holds a route template such as `/channels/{channel_id}/messages` (OB-N02).
- [ ] Tests for `TokenSanitizer` and for masking in logged exceptions (OB-N03).
- [ ] Eviction metric recorded (OB-F05).

## 11. Open questions
- Should `discord.route` use `DiscordRoute.Template` while the bucket hash moves to a separate
  `discord.bucket` tag, which is also unbounded? Provisional: use the template only, and keep the bucket
  hash in debug logs.
