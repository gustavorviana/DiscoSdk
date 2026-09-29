# PRD-API-003 — Gateway connection

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-003](../specs/api-003-gateway-connection.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [events/gateway](https://docs.discord.com/developers/events/gateway), [events/overview](https://docs.discord.com/developers/events/overview) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

<!-- Gateway opcodes and close codes come from topics/opcodes-and-status-codes, which PRD-API-001 owns as a page. -->

## 1. Problem
Bots receive everything through long-lived WebSocket sessions. A session has to identify with intents,
heartbeat with jitter, detect zombie connections, resume after drops, respect the identify concurrency
and the 120-per-60-seconds send limit, and split into shards as guild count grows. Getting any of this
wrong means missed events, reconnect storms or a locked-out token.

## 2. Goals
- Connect, identify, heartbeat, resume and reconnect automatically, and stop only on fatal close codes.
- Compute the shard count and the identify concurrency from `GET /gateway/bot`.
- Validate intents up front and tell the bot author which privileged intents a handler needs.
- Keep a production-safe footprint: bounded queues, zlib-stream decompression, clean shutdown inside the
  Kubernetes grace period.

## 3. Out of scope
- Handling of individual dispatch events: [PRD-API-004](api-004-gateway-events.md). Dispatch pipeline internals: [PRD-SDK-02](sdk-02-event-dispatch.md).
- The voice gateway and voice opcodes: [PRD-API-028](api-028-voice.md).
- The ETF encoding. JSON is the only supported encoding (see GW-F13).

## 4. Usage scenarios
- As a bot author, when Discord asks for a reconnect (op 7), my handlers keep running and no events are
  lost, thanks to resume.
- When my bot joins its 2,500th guild, the next start picks up the recommended shard count without a code
  change.
- When I forget the `MessageContent` intent, I get a warning at registration instead of empty strings at runtime.
- When the pod receives SIGTERM, every shard closes within `CloseTimeout`.

## 5. Desired developer experience

```csharp
var client = DiscordClientBuilder.Create(token)
    .WithIntents(DiscordIntent.Guilds | DiscordIntent.GuildMessages | DiscordIntent.MessageContent)
    .WithTotalShards(null)               // auto from /gateway/bot
    .WithAutoReconnect(true)
    .Build();

client.GatewayDisconnected += e =>
{
    logger.LogWarning(e.Exception, "Shard {Id} lost; reconnecting: {Retry}", e.Shard.Id, e.WillReconnect);
    return Task.CompletedTask;
};
await client.StartAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| GW-F01 | Connection lifecycle: connect → Hello → Identify/Resume → Ready → dispatch | — | Must | Implemented | `Shard`, `DefaultGatewaySocket`, `ShardPool` |
| GW-F02 | Gateway URL query parameters (`v=10`, `encoding=json`, `compress`) | — | Must | Implemented | `DiscordGatewayUri` |
| GW-F03 | Heartbeat with first-beat jitter, heartbeat ACK tracking and zombie detection | — | Must | Implemented | `Shard` heartbeat loop, `DiscordClientConfig.HeartbeatJitter` |
| GW-F04 | Answer server heartbeat requests (op 1 received) immediately | — | Must | Implemented | `Shard` system-message handler |
| GW-F05 | Identify payload: token, intents, properties, `large_threshold`, `shard` | — | Must | Implemented | `Shard.SendIdentifyAsync`, `DeviceInfo.CreateDefault` |
| GW-F06 | Initial presence in Identify | — | Could | Missing | Presence can only be set after Ready (`IDiscordClient.UpdatePresence` → op 3). |
| GW-F07 | Resume with `session_id`, `seq` and `resume_gateway_url` | — | Must | Implemented | `Shard` (`_sessionId`, `_resumeGatewayUrl`, `_preferResume`) |
| GW-F08 | Reconnect with exponential backoff and jitter, bounded attempts, optional manual reconnect | — | Must | Implemented | `Shard.ReconnectWithBackoffAsync`, `DiscordClientConfig.MaxReconnectAttempts`, `DiscordClientConfig.AutoReconnect`, `IDiscordClient.ReconnectAsync` (client-wide; the `ReconnectShardAsync` mentioned in the `AutoReconnect` XML docs does not exist) |
| GW-F09 | Fatal close codes stop reconnecting and surface to the bot | — | Must | Implemented | `GatewayExceptions.IsFatalCloseCode`, `IShardEventListener.OnFatalAsync`, `IDiscordClient.GatewayDisconnected` |
| GW-F10 | Gateway send limit: 120 commands per 60 s per connection, with heartbeat priority | — | Must | Implemented | `DefaultGatewaySocket` outbound token bucket (commit `fa4a4e5`) |
| GW-F11 | Transport compression `zlib-stream` | — | Must | Implemented | `GatewayZlibDecompress`, `GatewayCompressMode.ZlibStream` |
| GW-F12 | Transport compression `zstd-stream` | — | Could | Missing | Deferred to a companion package, to keep the native zstd dependency out of the core (see §11). |
| GW-F13 | ETF encoding | — | Could | Excluded | JSON only; ETF needs an Erlang term codec and brings no benefit on .NET. |
| GW-F14 | Payload compression (`compress: true` in Identify) | — | Could | Missing | Redundant with transport compression. |
| GW-F15 | Sharding: `[shard_id, num_shards]`, `shard_id = (guild_id >> 22) % num_shards` routing | — | Must | Implemented | `ShardPool`, `Shard`, `DiscordClientConfig.TotalShards` |
| GW-F16 | Identify concurrency buckets (`max_concurrency`, rate-limit key `shard_id % max_concurrency`) | — | Must | Partial | `IdentifyGate.SetMaxConcurrency` ← `ShardPool`: up to `max_concurrency` identifies run at once, but a permit is released on READY rather than after Discord's 5-second window, and permits are not keyed by `shard_id % max_concurrency` |
| GW-F17 | Session start limit (`total`, `remaining`, `reset_after`) | — | Must | Implemented | `SessionStartLimit`, `ShardPool` |
| GW-F18 | Sharding for large bots (shard count a multiple of 16) | — | Should | Implemented | Uses the recommended `shards` from `GET /gateway/bot`; manual override with `WithTotalShards`. |
| GW-F19 | Privileged intents: warn when a handler needs an intent that is not enabled | — | Must | Implemented | `RequiresIntentAttribute`, `EventHandlerIntentWarnTracker`, `IntentGuard` (commits `5c9c042`, `43d6256`) |
| GW-F20 | Message Content intent: empty-content behaviour without the intent | — | Must | Implemented | `IntentGuard` (warn once, returns empty) |
| GW-F21 | Guild availability (`unavailable` guilds in Ready, lazy `GUILD_CREATE`, outages) | — | Must | Implemented | `GuildManager`, `ReadyPayload` |
| GW-F22 | State tracking from dispatches (caches) | — | Must | Implemented | See [PRD-SDK-04](sdk-04-caching.md) |
| GW-F23 | Graceful shutdown within a bounded close handshake | — | Must | Implemented | `DiscordClientConfig.CloseTimeout`, `Shard.StopAsync` |
| GW-F24 | Backpressure: bounded per-shard dispatch queue | — | Must | Implemented | `DiscordClientConfig.EventProcessorQueueCapacity`, `ShardEventDispatcher` |
| GW-F25 | HELLO timeout (dead connection detected before HELLO) | — | Should | Implemented | `DiscordClientConfig.HelloTimeout` |
| GW-F26 | Compliant WebSocket User-Agent | — | Must | Implemented | `DiscordClientConfig.GatewayUserAgent` |
| GW-F27 | Get Gateway | `GET /gateway` | Should | Implemented | `IDiscordClient.Ping()` → `DiscordClient.Ping` |
| GW-F28 | Get Gateway Bot | `GET /gateway/bot` | Must | Implemented | `DiscordGatewayClient.GetGatewayBotInfoAsync` ← `DiscordClient` start-up (shard count, `SessionStartLimit`) |
| GW-F29 | Intent `GUILDS` | `intent:GUILDS` | Must | Implemented | `DiscordIntent.Guilds` |
| GW-F30 | Intent `GUILD_MEMBERS` | `intent:GUILD_MEMBERS` | Must | Implemented | `DiscordIntent.GuildMembers` |
| GW-F31 | Intent `GUILD_MODERATION` | `intent:GUILD_MODERATION` | Must | Implemented | `DiscordIntent.GuildModeration` |
| GW-F32 | Intent `GUILD_EXPRESSIONS` | `intent:GUILD_EXPRESSIONS` | Must | Implemented | `DiscordIntent.GuildExpressions` |
| GW-F33 | Intent `GUILD_INTEGRATIONS` | `intent:GUILD_INTEGRATIONS` | Must | Implemented | `DiscordIntent.GuildIntegrations` |
| GW-F34 | Intent `GUILD_WEBHOOKS` | `intent:GUILD_WEBHOOKS` | Must | Implemented | `DiscordIntent.GuildWebhooks` |
| GW-F35 | Intent `GUILD_INVITES` | `intent:GUILD_INVITES` | Must | Implemented | `DiscordIntent.GuildInvites` |
| GW-F36 | Intent `GUILD_VOICE_STATES` | `intent:GUILD_VOICE_STATES` | Must | Implemented | `DiscordIntent.GuildVoiceStates` |
| GW-F37 | Intent `GUILD_PRESENCES` | `intent:GUILD_PRESENCES` | Must | Implemented | `DiscordIntent.GuildPresences` |
| GW-F38 | Intent `GUILD_MESSAGES` | `intent:GUILD_MESSAGES` | Must | Implemented | `DiscordIntent.GuildMessages` |
| GW-F39 | Intent `GUILD_MESSAGE_REACTIONS` | `intent:GUILD_MESSAGE_REACTIONS` | Must | Implemented | `DiscordIntent.GuildMessageReactions` |
| GW-F40 | Intent `GUILD_MESSAGE_TYPING` | `intent:GUILD_MESSAGE_TYPING` | Must | Implemented | `DiscordIntent.GuildMessageTyping` |
| GW-F41 | Intent `DIRECT_MESSAGES` | `intent:DIRECT_MESSAGES` | Must | Implemented | `DiscordIntent.DirectMessages` |
| GW-F42 | Intent `DIRECT_MESSAGE_REACTIONS` | `intent:DIRECT_MESSAGE_REACTIONS` | Must | Implemented | `DiscordIntent.DirectMessageReactions` |
| GW-F43 | Intent `DIRECT_MESSAGE_TYPING` | `intent:DIRECT_MESSAGE_TYPING` | Must | Implemented | `DiscordIntent.DirectMessageTyping` |
| GW-F44 | Intent `MESSAGE_CONTENT` | `intent:MESSAGE_CONTENT` | Must | Implemented | `DiscordIntent.MessageContent` |
| GW-F45 | Intent `GUILD_SCHEDULED_EVENTS` | `intent:GUILD_SCHEDULED_EVENTS` | Must | Implemented | `DiscordIntent.GuildScheduledEvents` |
| GW-F46 | Intent `AUTO_MODERATION_CONFIGURATION` | `intent:AUTO_MODERATION_CONFIGURATION` | Must | Implemented | `DiscordIntent.AutoModerationConfiguration` |
| GW-F47 | Intent `AUTO_MODERATION_EXECUTION` | `intent:AUTO_MODERATION_EXECUTION` | Must | Implemented | `DiscordIntent.AutoModerationExecution` |
| GW-F48 | Intent `GUILD_MESSAGE_POLLS` | `intent:GUILD_MESSAGE_POLLS` | Must | Implemented | `DiscordIntent.GuildMessagePolls` |
| GW-F49 | Intent `DIRECT_MESSAGE_POLLS` | `intent:DIRECT_MESSAGE_POLLS` | Must | Implemented | `DiscordIntent.DirectMessagePolls` |
| GW-F50 | Gateway opcode 0 — Dispatch | `op:0` | Must | Implemented | `OpCodes.Dispatch` → `ShardEventDispatcher` |
| GW-F51 | Gateway opcode 1 — Heartbeat | `op:1` | Must | Implemented | `OpCodes.Heartbeat` → `Shard` heartbeat loop |
| GW-F52 | Gateway opcode 2 — Identify | `op:2` | Must | Implemented | `OpCodes.Identify` → `Shard.SendIdentifyAsync` |
| GW-F53 | Gateway opcode 3 — Presence Update | `op:3` | Must | Implemented | `IDiscordClient.UpdatePresence()` → `UpdatePresenceAction` → `OpCodes.PresenceUpdate` |
| GW-F54 | Gateway opcode 4 — Voice State Update | `op:4` | Must | Partial | `OpCodes.VoiceStateUpdate` is declared but never sent; joining voice is roadmap (PRD-API-028) |
| GW-F55 | Gateway opcode 6 — Resume | `op:6` | Must | Implemented | `OpCodes.Resume` → `Shard` |
| GW-F56 | Gateway opcode 7 — Reconnect | `op:7` | Must | Implemented | `OpCodes.Reconnect` → `Shard` (always resumable) |
| GW-F57 | Gateway opcode 8 — Request Guild Members | `op:8` | Must | Implemented | `IGuildMembers.Request()` → `IRequestGuildMembersAction` → `OpCodes.RequestGuildMembers`, `MemberChunkCoordinator` |
| GW-F58 | Gateway opcode 9 — Invalid Session | `op:9` | Must | Implemented | `OpCodes.InvalidSession` → `Shard` (drops the session when `d` is false) |
| GW-F59 | Gateway opcode 10 — Hello | `op:10` | Must | Implemented | `OpCodes.Hello` → `Shard` (heartbeat interval, `DiscordClientConfig.HelloTimeout`) |
| GW-F60 | Gateway opcode 11 — Heartbeat ACK | `op:11` | Must | Implemented | `OpCodes.HeartbeatAck` → `Shard` (zombie detection) |
| GW-F61 | Gateway opcode 31 — Request Soundboard Sounds | `op:31` | Should | Partial | `OpCodes.RequestSoundboardSounds` is declared but no public API sends it, and the `SOUNDBOARD_SOUNDS` reply is not dispatched (PRD-API-004) |
| GW-F62 | Gateway opcode 43 — Request Channel Info | `op:43` | Could | Missing | — |
| GW-F63 | Close code 4000 — Unknown error | `close:4000` | Must | Implemented | Reconnect with Resume → `Shard.ReconnectWithBackoffAsync` |
| GW-F64 | Close code 4001 — Unknown opcode | `close:4001` | Must | Implemented | Reconnect with Resume → `Shard.ReconnectWithBackoffAsync` |
| GW-F65 | Close code 4002 — Decode error | `close:4002` | Must | Implemented | Reconnect with Resume → `Shard.ReconnectWithBackoffAsync` |
| GW-F66 | Close code 4003 — Not authenticated | `close:4003` | Must | Implemented | Reconnect with Resume → `Shard.ReconnectWithBackoffAsync` |
| GW-F67 | Close code 4004 — Authentication failed | `close:4004` | Must | Implemented | Fatal: `GatewayExceptions.IsFatalCloseCode` → `IShardEventListener.OnFatalAsync`, no reconnect |
| GW-F68 | Close code 4005 — Already authenticated | `close:4005` | Must | Implemented | Reconnect with Resume → `Shard.ReconnectWithBackoffAsync` |
| GW-F69 | Close code 4007 — Invalid seq | `close:4007` | Must | Partial | Reconnects, but tries Resume first (Discord requires a new session); recovers after the resulting INVALID_SESSION → `Shard` |
| GW-F70 | Close code 4008 — Rate limited | `close:4008` | Must | Implemented | Reconnect with Resume → `Shard.ReconnectWithBackoffAsync` |
| GW-F71 | Close code 4009 — Session timed out | `close:4009` | Must | Partial | Reconnects, but tries Resume first (Discord requires a new session); recovers after the resulting INVALID_SESSION → `Shard` |
| GW-F72 | Close code 4010 — Invalid shard | `close:4010` | Must | Implemented | Fatal: `GatewayExceptions.IsFatalCloseCode` → `IShardEventListener.OnFatalAsync`, no reconnect |
| GW-F73 | Close code 4011 — Sharding required | `close:4011` | Must | Implemented | Fatal: `GatewayExceptions.IsFatalCloseCode` → `IShardEventListener.OnFatalAsync`, no reconnect |
| GW-F74 | Close code 4012 — Invalid API version | `close:4012` | Must | Implemented | Fatal: `GatewayExceptions.IsFatalCloseCode` → `IShardEventListener.OnFatalAsync`, no reconnect |
| GW-F75 | Close code 4013 — Invalid intent(s) | `close:4013` | Must | Implemented | Fatal: `GatewayExceptions.IsFatalCloseCode` → `IShardEventListener.OnFatalAsync`, no reconnect |
| GW-F76 | Close code 4014 — Disallowed intent(s) | `close:4014` | Must | Implemented | Fatal: `GatewayExceptions.IsFatalCloseCode` → `IShardEventListener.OnFatalAsync`, no reconnect |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| GW-N01 | The zlib-stream receive path decompresses through a `System.IO.Pipelines` pipe (`GatewayZlibDecompress`) and keeps one inflater per connection, as the shared zlib context requires. |
| GW-N02 | Reconnect backoff is capped at 900 s with ±20% jitter, so a fleet does not reconnect in sync. |
| GW-N03 | Shutdown of all shards finishes within `CloseTimeout` (5 s default), below the Kubernetes default grace period of 30 s. |
| GW-N04 | Heartbeat latency, reconnects and events received are exported as metrics (PRD-SDK-05). |
| GW-N05 | The token never appears in logs (Identify payload excluded from trace logging). |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Intents | — (required) | `DiscordIntent` flags | `WithIntents` |
| Total shards | `null` (auto) | ≥ 1 | `WithTotalShards` |
| Compression | `ZlibStream` | `None`, `ZlibStream` | `WithGatewayCompressMode` |
| Dispatch queue capacity per shard | 100 | ≥ 1 | `WithEventProcessorQueueCapacity` |
| Reconnect delay | 5 s | > 0 | `WithReconnectDelay` |
| Max reconnect attempts | 20 | ≤ 0 means unlimited | `WithMaxReconnectAttempts` |
| Auto reconnect | `true` | bool | `WithAutoReconnect` |
| Large threshold | 250 | 50–250 | `DiscordClientConfig.LargeThreshold` |
| Heartbeat jitter | 1.0 | 0–1 | `DiscordClientConfig.HeartbeatJitter` |
| Reconnect backoff jitter | 0.2 | 0–1 | `DiscordClientConfig.ReconnectBackoffJitter` |
| HELLO timeout | 30 s | > 0 | `DiscordClientConfig.HelloTimeout` |
| Close timeout | 5 s | > 0 | `WithCloseTimeout` |
| Gateway User-Agent | `DiscordBot (https://github.com/gustavorviana/DiscoSdk, 1.0)` | Discord format | `WithGatewayUserAgent` |

## 9. Compatibility
- New `GatewayCompressMode` members (`ZstdStream`) are additive.
- Discord is phasing out gateway v9 behaviours. The SDK pins `v=10`.

## 10. Acceptance criteria
- [x] Identify is sent after HELLO, and Resume replaces it when a session exists (`ShardIdentifyFlowTests`, `ShardReconnectTests`).
- [x] Heartbeat jitter and missed-ACK zombie detection (`ShardHeartbeatTests`).
- [x] Fatal close codes stop the shard and surface `OnFatalAsync` (`ShardFatalErrorTests`, `WaitShutdownFatalTests`).
- [x] Identify concurrency is capped at `max_concurrency` (`IdentifyGateTests`, `ShardIdentifyGateTests`).
- [ ] Identify permits honour the 5-second window per rate-limit key (GW-F16).
- [x] `AutoReconnect = false` leaves the shard disconnected until a manual reconnect (`ShardAutoReconnectTests`).
- [x] zlib-stream frames decompress across message boundaries (`GatewayZlibDecompressTests`).
- [ ] Close codes 4007 and 4009 go straight to Identify instead of trying Resume first.
- [ ] Initial presence can be set in Identify (GW-F06).

## 11. Open questions
- zstd-stream: a companion package (`DiscoSdk.Compression.Zstd`) plugging into `GatewayDecompressFactory`?
  Provisional: yes, with an extension point instead of an enum value in the core.
- Should `op:43` (Request Channel Info) get a public method now that `CHANNEL_INFO` exists? Provisional: yes,
  together with the voice-status events in PRD-API-028.
