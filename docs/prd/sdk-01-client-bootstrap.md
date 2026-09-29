# PRD-SDK-01 — Client bootstrap, configuration and modules

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-SDK-01](../specs/sdk-01-client-bootstrap.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — (SDK framework feature) |
| **Last updated** | 2026-09-29 |

## 1. Problem
A bot author needs one obvious way to configure and start a bot: token, intents, sharding, reconnect
policy, logging, JSON, caches, command discovery, event handlers and extension points. Hosting also
matters. Bots run as console apps, inside ASP.NET Core, or in the Generic Host, and they need dependency
injection for their own services (database, HTTP clients).

## 2. Goals
- A fluent `DiscordClientBuilder` with safe defaults and explicit required settings.
- Lifecycle APIs (`StartAsync`, `WaitReadyAsync`, `StopAsync`, `WaitShutdownAsync`) and lifecycle events.
- Modules that hook into pre-init, ready, shutdown, per-event dependency-injection scopes and the command
  update window.
- Bot authors register their own services and resolve them in handlers.

## 3. Out of scope
- Gateway tuning semantics: [PRD-API-003](api-003-gateway-connection.md). Handler execution:
  [PRD-SDK-02](sdk-02-event-dispatch.md). Commands: [PRD-SDK-03](sdk-03-command-framework.md). Caches:
  [PRD-SDK-04](sdk-04-caching.md).

## 4. Usage scenarios
- As a bot author, I build the client in `Program.cs` with intents and handlers, then start it.
- My handler needs `IDbContextFactory<AppDb>`: I register it once and inject it into the constructor.
- In Kubernetes, SIGTERM stops the client and waits for shutdown within the grace period.

## 5. Desired developer experience

```csharp
var client = DiscordClientBuilder.Create(token)
    .WithIntents(DiscordIntent.AllUnprivileged | DiscordIntent.MessageContent)
    .WithLogger(loggerFactory.CreateLogger("DiscoSdk"))
    .WithSlashCommands(typeof(Program).Assembly)
    .AddEventHandler<WelcomeHandler>()
    .AddModule<MetricsModule>()
    .Build();

await client.StartAsync();
await client.WaitShutdownAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| CB-F01 | Fluent builder with required token and intents | — | Must | Implemented | `DiscordClientBuilder.Create`, `DiscordClientBuilder.Build` (throws without intents) |
| CB-F02 | Gateway and reconnect options (shards, compression, queue capacity, reconnect delay/attempts/auto, close timeout, User-Agent) | — | Must | Implemented | `DiscordClientBuilder.WithTotalShards`, `DiscordClientBuilder.WithGatewayCompressMode`, `DiscordClientBuilder.WithEventProcessorQueueCapacity`, `DiscordClientBuilder.WithReconnectDelay`, `DiscordClientBuilder.WithMaxReconnectAttempts`, `DiscordClientBuilder.WithAutoReconnect`, `DiscordClientBuilder.WithCloseTimeout`, `DiscordClientBuilder.WithGatewayUserAgent` |
| CB-F03 | Logging through `Microsoft.Extensions.Logging` | — | Must | Partial | `DiscordClientBuilder.WithLogger(ILogger)` accepts a single logger, so there is no `ILoggerFactory` and no per-component categories |
| CB-F04 | Injectable clock | — | Should | Implemented | `DiscordClientBuilder.WithTimeProvider` |
| CB-F05 | JSON options override | — | Should | Implemented | `DiscordClientBuilder.WithJsonOptions` (defaults: `DiscoJson.Create`) |
| CB-F06 | Event handler registration by type or instance, with constructor injection | — | Must | Implemented | `DiscordClientBuilder.AddEventHandler` → `DiscoFactory` (`ActivatorUtilities`) |
| CB-F07 | Modules: lifetime hooks, per-event dependency-injection scope hook, command update window hook | — | Must | Implemented | `ILifetimeDiscoModule`, `IDependencyScopeDiscoModule`, `ICommandsUpdateWindowModule`, `DiscordClientBuilder.AddModule` |
| CB-F08 | Register the bot author's own services into the SDK container | — | Must | Missing | The builder owns a private `ServiceCollection` with no `ConfigureServices` hook, so `[FromServices]` and handler constructors can only resolve SDK services |
| CB-F09 | Generic Host / ASP.NET Core integration (`IServiceCollection.AddDiscoSdk`, hosted service, external `IServiceProvider`) | — | Should | Missing | — |
| CB-F10 | Client accessor for services that are built before the client | — | Should | Implemented | `IDiscordClientAccessor` |
| CB-F11 | Lifecycle: start, stop, reconnect, wait ready, wait shutdown, ready state and event | — | Must | Implemented | `IDiscordClient.StartAsync`, `IDiscordClient.StopAsync`, `IDiscordClient.ReconnectAsync`, `IDiscordClient.WaitReadyAsync`, `IDiscordClient.WaitShutdownAsync`, `IDiscordClient.IsReady`, `IDiscordClient.OnReady` |
| CB-F12 | Unhandled error surfacing | — | Must | Implemented | `IDiscordClient.UnhandledError` (`UnhandledErrorEventArgs`) |
| CB-F13 | Webhook-only client (no token, no gateway) | — | Should | Implemented | `DiscordWebhookClientBuilder` (PRD-API-026) |
| CB-F14 | Secret-safe configuration printing | — | Must | Implemented | `DiscordClientConfig.ToString` → `TokenSanitizer.Mask` |
| CB-F15 | Command discovery, localization, parameter providers and object conversion | — | Must | Implemented | `DiscordClientBuilder.WithSlashCommands`, `DiscordClientBuilder.WithContextMenuCommands`, `DiscordClientBuilder.WithCommandLocalization`, `DiscordClientBuilder.WithParamProvider`, `DiscordClientBuilder.WithObjectConverter` (PRD-SDK-03) |
| CB-F16 | Cache configuration | — | Must | Implemented | `DiscordClientBuilder.WithMemberCachePolicy`, `DiscordClientBuilder.WithPresenceCache`, `DiscordClientBuilder.WithStickerCache` (PRD-SDK-04) |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| CB-N01 | `Build()` performs no network I/O. The first call is made in `StartAsync()`. |
| CB-N02 | Tests can replace the gateway socket factory and the REST client (internal builder hooks through `InternalsVisibleTo`). |
| CB-N03 | Shutdown (`StopAsync`) completes within `CloseTimeout` per shard. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Token | required | non-empty | `DiscordClientBuilder.Create(token)` |
| Intents | required | `DiscordIntent` | `WithIntents` |
| Total shards | auto | ≥ 1 | `WithTotalShards` |
| Compression | `ZlibStream` | `None`, `ZlibStream` | `WithGatewayCompressMode` |
| Event queue capacity per shard | 100 | ≥ 1 | `WithEventProcessorQueueCapacity` |
| Reconnect delay | 5 s | > 0 | `WithReconnectDelay` |
| Max reconnect attempts | 20 | ≤ 0 unlimited | `WithMaxReconnectAttempts` |
| Auto reconnect | true | bool | `WithAutoReconnect` |
| Close timeout | 5 s | > 0 | `WithCloseTimeout` |
| Gateway User-Agent | `DiscordBot (https://github.com/gustavorviana/DiscoSdk, 1.0)` | Discord format | `WithGatewayUserAgent` |
| Logger | `NullLogger` | `ILogger` | `WithLogger` |
| Time provider | `TimeProvider.System` | `TimeProvider` | `WithTimeProvider` |
| JSON options | `DiscoJson.Create()` | `JsonSerializerOptions` | `WithJsonOptions` |
| Large threshold, heartbeat jitter, backoff jitter, HELLO timeout | 250, 1.0, 0.2, 30 s | see PRD-API-003 | `DiscordClientConfig` only (not on the builder) |

## 9. Compatibility
Adding `ConfigureServices(Action<IServiceCollection>)` and an `AddDiscoSdk` hosting package is additive.
Replacing `WithLogger(ILogger)` with `WithLoggerFactory` should keep the old overload.

## 10. Acceptance criteria
- [x] Handlers and command types resolve with dependency injection (`CommandRegistryBuilderTests`, `ClientMessageIntegrationTests`).
- [ ] A test asserts that `Build()` without intents throws.
- [x] Lifecycle waits and fatal shutdown (`WaitShutdownFatalTests`, `GatewayDisconnectedEventTests`).
- [ ] A test asserts that `DiscordClientConfig.ToString()` masks the token (no dedicated test today).
- [ ] Bot authors register their own services (CB-F08).
- [ ] Generic Host integration (CB-F09).

## 11. Open questions
- `ConfigureServices` on the builder, or accept an external `IServiceProvider`? Provisional: both.
  `ConfigureServices` for console bots, and `IServiceCollection.AddDiscoSdk(o => …)` in a
  `DiscoSdk.Extensions.Hosting` package that registers a hosted service for Generic Host apps.
