# SPEC-API-004 — Gateway events

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-004](../prd/api-004-gateway-events.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [events/gateway-events](https://docs.discord.com/developers/events/gateway-events) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`ShardEventDispatcher` drains each shard's queue and calls `DiscordEventDispatcher.ProcessEventAsync`.
That method opens a `discosdk.gateway.dispatch` activity and switches on `t` (the event name). Each case
has a `Process…Async` method that parses the payload, updates caches and managers (guilds, channels,
members, presences, stickers, DMs), builds a context wrapper and calls
`HandleAllAsync<THandler, TContext>(context)`. `HandleAllAsync` resolves every registered handler for that
interface and runs them in order (SPEC-SDK-02). `READY` and `RESUMED` are consumed by `Shard` itself.

## 2. Projects and dependencies
- `DiscoSdk/Events` (`IDiscordEventHandler`, the handler interfaces, `RequiresIntentAttribute`) and
  `DiscoSdk/Contexts/**` (the context interfaces).
- `DiscoSdk.Hosting/Gateway/Events/*` and `DiscoSdk.Hosting/Contexts/**` (the wrappers).

## 3. Models and contracts
- Each handler is an `IDiscordEventHandler<TContext>` with `Task HandleAsync(TContext context)`, and
  each context implements `IContext` (`Client`).
- Contexts expose domain interfaces, never wire models: for example
  `IGuildMemberAddContext { IMember Member; IGuild Guild; }`.
- Payloads are read through `JsonElementParser` for small events, and deserialised into
  `DiscoSdk.Hosting/Models` types for rich ones such as guilds, messages and channels.

## 4. Components
| Type | Responsibility |
|---|---|
| `ShardEventDispatcher` | Per-shard bounded channel and worker. Forwards op 0 frames. |
| `DiscordEventDispatcher.ProcessEventAsync` | Event name → `Process…Async`. Errors are caught and logged at `Error`. |
| `DiscordEventDispatcher.HandleAllAsync<THandler, TContext>` | Resolves handlers, applies `[FireAndForget]`, dependency-injection scopes and chain-break (SPEC-SDK-02). |
| `GuildManager`, `ChannelManager`, `MemberManager`, `PresenceManager`, `StickerManager`, `DmChannelRepository`, `UserRepository` | Cache updates before handlers run (SPEC-SDK-04). |
| `EventHandlerIntentWarnTracker` | Warns once per missing intent for registered handlers. |

## 5. Public API
- Register handlers with `DiscordClientBuilder.AddEventHandler<T>()`, `AddEventHandler(Type)` or
  `AddEventHandler(IDiscordEventHandler)`. Types are created through `DiscoFactory`
  (`ActivatorUtilities.CreateInstance`), so constructor injection works.
- Handler interfaces: one per implemented event (see PRD §6). `INTERACTION_CREATE` fans out to
  `IInteractionCreateHandler` and to the typed handlers (`IApplicationCommandHandler`,
  `IUserCommandHandler`, `IMessageCommandHandler`, `IAutoCompleteHandler`, `IComponentInteractionHandler`,
  `IModalSubmitHandler`).
- Lifecycle: `IDiscordClient.OnReady`, `IsReady`, `WaitReadyAsync`.

## 6. Discord surface
There are 79 receive events at the baseline. Intent gating follows Discord's list (`GUILDS`,
`GUILD_MEMBERS`, …). Handlers declare their intents with `[RequiresIntent]`.

## 7. Flows
1. The shard queue yields a `ReceivedGatewayMessage` with `t` set.
2. `ProcessEventAsync` → the `Process…Async` for that name.
3. Parse the payload → update caches (for example, add the member to `MemberManager` on `GUILD_MEMBER_ADD`).
4. Build the context wrapper → `HandleAllAsync`.
5. If an exception escapes, `Logger.Log(Error, ex, "Error processing event {EventType}")`. The shard continues.

## 8. Concurrency and lifecycle
- Events from one shard are processed sequentially, in gateway order, unless a handler opts into
  `[FireAndForget]`.
- Different shards process events in parallel. Cache structures are concurrent (SPEC-SDK-04).

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Event name not in the `switch` (new Discord event) | Ignored silently. **Proposed:** log once at `Debug` with the name. |
| Handler throws | Logged at `Error`. Later handlers in the chain still run (SPEC-SDK-02). |
| Event references an uncached guild or channel | Context lazily exposes IDs. Accessing `Guild` fetches or returns what is cached, per entity (SPEC-SDK-04). |
| `MESSAGE_CREATE` without `MessageContent` intent | `Content` is empty, and `IntentGuard` warns once. |

## 10. Observability
- Span `discosdk.gateway.dispatch` (tag `event_type`) per event.
- Counters: gateway events received, handler invocations (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `GuildDispatchTests`, `GuildExtrasDispatchTests` | GE-F24 – GE-F27, GE-F30 – GE-F32 |
| `ChannelDispatchTests`, `ThreadDispatchTests`, `UserPinsWebhooksDispatchTests` | GE-F08 – GE-F10, GE-F14 – GE-F20, GE-F65, GE-F69 |
| `MemberDispatchTests`, `MemberCacheDispatchTests`, `MemberManagerSeedTests` | GE-F33 – GE-F36 |
| `RoleDispatchTests`, `BanDispatchTests` | GE-F28, GE-F29, GE-F37 – GE-F39 |
| `MessageDispatchTests`, `MessageVariantsDispatchTests`, `ReactionDispatchTests`, `PollVoteDispatchTests`, `TypingDispatchTests` | GE-F55 – GE-F64, GE-F77, GE-F78 |
| `InviteDispatchTests`, `IntegrationDispatchTests` | GE-F50 – GE-F54 |
| `AutoModerationDispatchTests` | GE-F04 – GE-F07 |
| `EntitlementDispatchTests`, `SubscriptionDispatchTests` | GE-F21 – GE-F23, GE-F74 – GE-F76 |
| `StageInstanceDispatchTests`, `GuildScheduledEventDispatchTests` | GE-F40 – GE-F44, GE-F71 – GE-F73 |
| `InteractionDispatchTests`, `DispatchByCommandTypeTests` | GE-F70 |
| `PresenceManagerTests`, `StickerCacheDispatchTests` | GE-F31, GE-F63 |
| `EventHandlerIntentGuardTests`, `MessageContentExemptionTests` | GE-N03 |
| `ShardIdentifyFlowTests` | GE-F01, GE-F02 |

## 12. History
| Commit | Change |
|---|---|
| `9f81c56` | Interface-first model split (contexts expose interfaces). |
| `5c9c042` | `[RequiresIntent]` registration guard. |
| `3abe313`, `c889665`, `16acd2d`, `743f5b4` | Member, presence and sticker caches fed by dispatch. |
| `b9ff1b6` | Dispatch spans and handler metrics. |
| `6322dc5`, `506fd73` | `[FireAndForget]` and chain-break. |

Next steps:
1. Add soundboard events and the `ISoundboardSound` cache (with PRD-API-024).
2. Add `VOICE_STATE_UPDATE` and a voice-state cache (with PRD-API-028).
3. Add `APPLICATION_COMMAND_PERMISSIONS_UPDATE`.
4. Add `RATE_LIMITED` metric and log.
5. Add a `default` branch that logs unknown events.

## 13. Decisions and rejected alternatives
- **One interface per event** rather than C# events or a string-keyed bus: this gives compile-time
  discovery, dependency-injection friendliness, and a place to attach intent metadata.
- **Update caches before handlers**: handlers always see post-event state, consistent with what a follow-up
  cache read returns.
