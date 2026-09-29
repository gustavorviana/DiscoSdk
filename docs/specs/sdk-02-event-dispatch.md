# SPEC-SDK-02 — Event dispatch pipeline

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-SDK-02](../prd/sdk-02-event-dispatch.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — |
| **Last updated** | 2026-09-29 |

## 1. Summary
Each shard enqueues op 0 frames into its own `ShardEventDispatcher`, a bounded `Channel` with a single
consumer. The consumer calls `DiscordEventDispatcher.ProcessEventAsync`, which switches on the event name
(SPEC-API-004), builds the context and calls `HandleAllAsync<THandler, TContext>`.
- `HandleAllAsync` looks up the handlers registered for `THandler` in registration order and runs each one
  through `SafeRunHandlerAsync`.
- `SafeRunHandlerAsync` either awaits `InvokeHandlerCoreAsync` or, with `[FireAndForget]`, starts it on the
  thread pool.
- `InvokeHandlerCoreAsync` creates an async dependency-injection scope, sets the context in
  `SdkContextProvider`, runs the `IDependencyScopeDiscoModule` hooks, calls
  `handler.HandleAsync(context, scope)`, and records the span and metrics. Exceptions are caught and logged.

## 2. Projects and dependencies
- `DiscoSdk`: `IDiscordEventHandler<TContext>`, `FireAndForgetAttribute`, `RequiresIntentAttribute`,
  `ISdkContextProvider`, `IDependencyScopeDiscoModule`.
- `DiscoSdk.Hosting`: `ShardEventDispatcher`, `DiscordEventDispatcher`, `FireAndForgetCache`,
  `EventHandlerIntentWarnTracker`, `InteractionHandle`, `SdkContextProvider`.
- Package: `System.Threading.Channels` (part of the base class library).

## 3. Models and contracts
- `IDiscordEventHandler<TContext>.HandleAsync(TContext context, IServiceProvider services)`.
- `InteractionHandle { Responded, SkipNextExecutions }`, scoped to one interaction dispatch chain.

## 4. Components
| Type | Responsibility |
|---|---|
| `ShardEventDispatcher` | `EnqueueAsync` (awaits when full); the worker drains and calls the dispatcher; `DisposeAsync` completes the writer and drains the backlog. |
| `DiscordEventDispatcher.Add` | Registers a handler once and indexes it by every `IDiscordEventHandler`-derived interface; warns about missing intents. |
| `HandleAllAsync` (event overload) | Runs all handlers; breaks on `SkipNextExecutions`. |
| `HandleAllAsync` (interaction overload) | Resets `SkipNextExecutions`, stops when `Responded` or skip is set. |
| `FireAndForgetCache` | Caches attribute lookups per handler type and interface (the class first, then the `HandleAsync` implementing that interface). |

## 5. Public API
`DiscordClientBuilder.AddEventHandler<T>()` (plus the type and instance overloads), `[FireAndForget]`,
`[FireAndForget(SkipNextExecutions = true)]`, `[RequiresIntent]`, `IDependencyScopeDiscoModule`,
`ISdkContextProvider` (resolve the current context inside scoped services).

## 6. Discord surface
None directly (it consumes op 0 dispatches).

## 7. Flows
1. The shard receives a frame, `ShardEventDispatcher.EnqueueAsync` stores it, and the worker dequeues it.
2. `ProcessEventAsync` updates the caches and builds the context.
3. For each handler of the interface, in registration order:
   - without the attribute, await the invocation;
   - with `[FireAndForget]`, run `Task.Run(invoke)` and continue immediately. A `SkipNextExecutions` flag is
     set synchronously before detaching, so the loop sees it.
4. Invocation: create the scope, run the scope hooks, call the handler, then record `HandlerLatency` and
   `HandlerInvocations` (outcome and exception type).

## 8. Concurrency and lifecycle
- One worker per shard: events are serialised per shard and parallel across shards.
- `[FireAndForget]` handlers run concurrently with later events. Ordering and cache snapshots are not
  guaranteed for them.
- Handler singletons can be called concurrently (fire-and-forget, or several shards).

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Handler throws | Logged at `Error` ("Error in {HandlerType}", or the fire-and-forget variant), the outcome is tagged `error`, and the next handler runs. |
| Queue full (slow handlers) | The receive loop awaits (backpressure). Heartbeats are unaffected (separate timer). |
| A `[FireAndForget]` interaction handler that does not defer within 3 s | Discord marks the interaction as failed. The handler must acknowledge first. |
| The same handler instance registered twice | Ignored (`_handlers.Contains`). |

## 10. Observability
- Span `discosdk.handler.invoke` (tags: handler type, event type, `discosdk.handler.fire_and_forget`).
- Histogram `HandlerLatency` and counter `HandlerInvocations` (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `ShardEventDispatcherTests` | ED-F01, ED-N01 |
| `FireAndForgetCacheTests` | ED-F05, ED-F06 |
| `DispatchByCommandTypeTests`, `InteractionDispatchTests` | ED-F06, ED-F07 |
| `HandlerInvocationsMetricTests` | ED-F08, ED-N04 |
| `EventHandlerIntentGuardTests`, `MessageContentExemptionTests` | ED-F09 |
| `ClientMessageIntegrationTests` | ED-F02, ED-F03 |

Implemented or Partial requirements without a covering test: ED-F04, ED-N02 – ED-N03.

## 12. History
| Commit | Change |
|---|---|
| `6322dc5` | `[FireAndForget]` attribute opt-in and per-shard worker. |
| `506fd73` | `SkipNextExecutions` chain break. |
| `b9ff1b6` | Handler instrumentation and spans. |
| `5c9c042` | `[RequiresIntent]` registration guard. |

## 13. Decisions and rejected alternatives
- **Opt-in fire-and-forget** rather than always parallel: ordered processing is the safe default for cache
  consistency and rate limits, so detaching is an explicit choice.
- **Singleton handlers plus a scope per invocation**: zero allocation per handler, with per-event
  services still available through `services`.
