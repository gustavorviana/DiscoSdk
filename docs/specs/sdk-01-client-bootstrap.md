# SPEC-SDK-01 — Client bootstrap, configuration and modules

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-SDK-01](../prd/sdk-01-client-bootstrap.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — |
| **Last updated** | 2026-09-29 |

## 1. Summary
`DiscordClientBuilder` collects settings into a private `ServiceCollection`. `Build()`:
1. creates `DiscordClientConfig`, JSON options, the object converter, the logger, the `TimeProvider`, the
   gateway socket factory and a `DiscordRestClient` (base `https://discord.com/api/v10`, see SPEC-API-001);
2. registers these services and the command services (registry, dispatchers, update factory);
3. builds the `IServiceProvider` and constructs `DiscordClient`;
4. sets `IDiscordClientAccessor`, then calls `InternalInit(modules, handlers)`, where modules and handlers
   are created through `DiscoFactory`.

`StartAsync()` makes the first network call. Lifetime modules and events are invoked at fixed points (§7).

## 2. Projects and dependencies
- `DiscoSdk`: `IDiscordClient`, `IDiscordClientAccessor`, `Modules/*` (`IDiscoModule`, `ILifetimeDiscoModule`,
  `IDependencyScopeDiscoModule`, `ICommandsUpdateWindowModule`), `UnhandledErrorEventArgs`, `TokenSanitizer`.
- `DiscoSdk.Hosting`: `DiscordClientBuilder`, `DiscordClientConfig`, `DiscordClient`, `DiscoFactory`,
  `DiscordClientAccessor`, `DiscordWebhookClientBuilder`.
- Packages: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging.Abstractions`.

## 3. Models and contracts
`DiscordClientConfig` is a mutable options class (required `Token` and `Intents`). Its `ToString()`
masks the token.

## 4. Components
| Type | Responsibility |
|---|---|
| `DiscordClientBuilder` | Fluent options, command scanners, handler and module lists, and internal test hooks (`WithGatewaySocketFactory`, `WithRestClient`). |
| `DiscoFactory` | Creates registered types with `ActivatorUtilities.CreateInstance(services, type)`, or uses instances as given. |
| `DiscordClient` | Implements `IDiscordClient`, owns `ShardPool`, the managers, REST clients and dispatcher, and implements `IShardEventListener`. |
| `DiscordClientAccessor` | Resolves `IDiscordClient` for services created before the client. |

## 5. Public API
See PRD §5 and §8. The events are `OnReady`, `UnhandledError`, `GatewayDisconnected`,
`GatewayReconnecting` and `CommandsUpdateWindowOpened`.

## 6. Discord surface
`GET /gateway/bot` at start-up (SPEC-API-003).

## 7. Flows
**Start**
1. `LogPrivilegedIntentReminder()`.
2. `GET /gateway/bot`.
3. For each `ILifetimeDiscoModule`: `OnPreInitializeAsync`. Modules that also implement `IDiscordEventHandler` are added to the dispatcher.
4. `ShardPool.SetGateway`, then `InitShardsAsync`.

**First full READY (runs once)**
1. Seed pending guilds (shard 0).
2. For each lifetime module: `OnGatewayReadyAsync`.
3. `InitSlashCommandsAsync`: the `ICommandsUpdateWindowModule` modules, then the `CommandsUpdateWindowOpened` handlers, then `session.ApplyAllAsync()`.
4. Raise `OnReady` and complete `WaitReadyAsync`.

**Stop**
1. Guarded against re-entry.
2. For each lifetime module: `OnShutdownAsync`.
3. `ShardPool.ClearShardsAsync`, then dispose.
4. Complete `WaitShutdownAsync`.

**Per event**
`IDependencyScopeDiscoModule.OnScopeCreatedAsync(context, scopeServices)` runs when the dispatcher creates a scope (SPEC-SDK-02).

## 8. Concurrency and lifecycle
- The builder is single-threaded, and `Build()` can be called once per builder instance.
- `StopAsync` is idempotent: concurrent callers await the same completion.
- `OnReady` fires once per client lifetime. Shard-level readiness is tracked through `IShard.IsReady` and `GatewayDisconnected`.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| `Build()` without intents | `InvalidOperationException` ("Intents are required…"). |
| Empty token | `ArgumentException` from the constructor. |
| A lifetime module throws in a hook | **Current:** swallowed silently (`catch { }`), with nothing logged. **Proposed:** log at `Error` and continue. |
| Handler needs a bot author's service | **Current:** cannot be registered (CB-F08), so construction fails with `InvalidOperationException` from `ActivatorUtilities`. |

## 10. Observability
- An `Information` log lists the privileged intents at start-up.
- Lifecycle metrics (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `CommandRegistryBuilderTests`, `ClientMessageIntegrationTests`, `ClientModalIntegrationTests` | CB-F06, CB-F15 |
| `PrivilegedIntentReminderTests` | CB-F02 (intent reminder) |
| `WaitShutdownFatalTests`, `GatewayDisconnectedEventTests` | CB-F11, CB-F12 |
| `MemberCachePolicyBuilderTests`, `PolicyPresetsTests`, `PresenceManagerTests` | CB-F16 |
| `ShardStopAsyncTests` | CB-N03 |

## 12. History
| Commit | Change |
|---|---|
| `649a310` | Scanner-based command registration wired through the builder. |
| `a9e57d6` | Close, reconnect and User-Agent options. |
| `3abe313`, `c889665`, `16acd2d` | Cache options on the builder. |
| `6322dc5` | Per-shard dispatch worker (queue capacity option). |

Next steps:
1. Add `ConfigureServices(Action<IServiceCollection>)`.
2. Add a `DiscoSdk.Extensions.Hosting` package with `AddDiscoSdk` and a hosted service.
3. Add `WithLoggerFactory`.
4. Log module hook failures.

## 13. Decisions and rejected alternatives
- **Private container**: it keeps the SDK's services isolated. The cost is that bot authors cannot add
  their own services (CB-F08). The proposed fix is an explicit hook, not sharing the host container
  implicitly.
