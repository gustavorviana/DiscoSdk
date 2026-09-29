# PRD-SDK-02 — Event dispatch pipeline

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-SDK-02](../specs/sdk-02-event-dispatch.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — (SDK framework feature) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Gateway events must reach many handlers:
- in a predictable order;
- without one slow or failing handler stalling a shard;
- with per-invocation dependency-injection scopes;
- with a way to run long work without blocking the next event;
- with interaction handlers that do not answer the same interaction twice.

## 2. Goals
- Per-shard ordered processing with bounded queues and backpressure.
- Handlers are registered once and resolved by interface, and each invocation gets its own dependency-injection scope.
- `[FireAndForget]` opt-in to detach long handlers, and `SkipNextExecutions` to break a chain.
- Failures are isolated, logged and measured.

## 3. Out of scope
- Which events exist, and their contexts: [PRD-API-004](api-004-gateway-events.md).
- Command routing: [PRD-SDK-03](sdk-03-command-framework.md).

## 4. Usage scenarios
- As a bot author, a `/report` command runs a 20-second query. I mark it `[FireAndForget]` and defer inside
  it, so other events on the shard keep flowing.
- Two message handlers run in registration order. The second one never sees a half-updated cache.
- A buggy handler throws; the error is logged with the handler type, and the shard keeps working.

## 5. Desired developer experience

```csharp
[FireAndForget(SkipNextExecutions = true)]
public sealed class SlowReport : IApplicationCommandHandler
{
    public async Task HandleAsync(ICommandContext context, IServiceProvider services)
    {
        await context.Defer().ExecuteAsync();
        var db = services.GetRequiredService<ReportService>();   // resolved from the invocation scope
        await context.Interaction.Edit().SetContent(await db.BuildAsync()).ExecuteAsync();
    }
}
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| ED-F01 | Per-shard bounded queue with one consumer (ordered per shard, backpressure when full) | — | Must | Implemented | `ShardEventDispatcher` (`BoundedChannelFullMode.Wait`), `DiscordClientConfig.EventProcessorQueueCapacity` |
| ED-F02 | Handler registration indexed by handler interface; duplicates ignored; registration order kept | — | Must | Implemented | `DiscordEventDispatcher.Add` |
| ED-F03 | Per-invocation dependency-injection scope passed to `HandleAsync(context, services)` | — | Must | Implemented | `IDiscordEventHandler`, `SdkContextProvider` (`ISdkContextProvider`) |
| ED-F04 | Scope hook for modules | — | Should | Implemented | `IDependencyScopeDiscoModule.OnScopeCreatedAsync` |
| ED-F05 | Detached execution with `[FireAndForget]` on a class or on `HandleAsync` | — | Must | Implemented | `FireAndForgetAttribute`, `FireAndForgetCache` (commit `6322dc5`) |
| ED-F06 | Chain break with `[FireAndForget(SkipNextExecutions = true)]` | — | Should | Implemented | `FireAndForgetAttribute.SkipNextExecutions` (commit `506fd73`) |
| ED-F07 | Interaction chain stops once an interaction is answered | — | Must | Implemented | `InteractionHandle.Responded` |
| ED-F08 | Handler failures isolated, logged and measured | — | Must | Implemented | `DiscordEventDispatcher` (logs at `Error` with the handler type) |
| ED-F09 | Intent warning at registration for gated handlers | — | Must | Implemented | `RequiresIntentAttribute`, `EventHandlerIntentWarnTracker` |
| ED-F10 | Unsubscribe or remove handlers at runtime | — | Could | Missing | Handlers are fixed after `Build()` (lifetime modules can add handlers in `OnPreInitializeAsync`) |
| ED-F11 | Handler ordering control (priority) | — | Could | Missing | Registration order only |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| ED-N01 | The receive loop never awaits a handler. It only enqueues. |
| ED-N02 | A failing handler never stops the shard or the chain. The exception is logged, and later handlers still run. |
| ED-N03 | Handler instances are created once (`DiscoFactory`) and shared, so they must be thread-safe when `[FireAndForget]` is used. |
| ED-N04 | Each handler invocation emits a `discosdk.handler.invoke` span plus latency and outcome metrics. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Queue capacity per shard | 100 | ≥ 1 | `DiscordClientBuilder.WithEventProcessorQueueCapacity` |
| Fire and forget | off | per handler | `[FireAndForget]` |

## 9. Compatibility
Adding priorities or runtime removal is additive. Changing the default ordering would be breaking.

## 10. Acceptance criteria
- [x] Events on one shard are processed in order, and the queue applies backpressure (`ShardEventDispatcherTests`).
- [x] `[FireAndForget]` detaches and `SkipNextExecutions` breaks the chain (`FireAndForgetCacheTests`, `DispatchByCommandTypeTests`).
- [x] Handler metrics and outcome tags (`HandlerInvocationsMetricTests`).
- [x] Intent guard at registration (`EventHandlerIntentGuardTests`).

## 11. Open questions
- Should handlers be resolved per invocation (scoped lifetime) instead of being singletons? Provisional:
  keep singletons for zero allocation, and document that per-request state belongs in the `services` scope.
