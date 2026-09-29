# SPEC-API-003 — Gateway connection

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-003](../prd/api-003-gateway-connection.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [events/gateway](https://docs.discord.com/developers/events/gateway), [topics/opcodes-and-status-codes](https://docs.discord.com/developers/topics/opcodes-and-status-codes) |
| **Last updated** | 2026-09-29 |

## 1. Summary
On `StartAsync`, `DiscordClient` calls `GET /gateway/bot`. It then sizes a `ShardPool`, using the
recommended shard count or `TotalShards`, and configures the `IdentifyGate` with `max_concurrency`.
Each `Shard` owns an `IGatewaySocket`, which is a `DefaultGatewaySocket` in production. The socket
decompresses zlib-stream frames and rate-limits outbound commands to 110 normal plus 10 heartbeat tokens
per 60 s. The shard runs a state machine:
`Connecting → Identifying → Ready → (Reconnecting | ConnectionLost | Disconnected)`.
Dispatch frames (op 0) are pushed into the shard's bounded `ShardEventDispatcher` channel, which is the
backpressure point, and are consumed by `DiscordEventDispatcher` (SPEC-SDK-02).

## 2. Projects and dependencies
- `DiscoSdk`: `IShard`, `ShardStatus`, `DiscordIntent`, `RequiresIntentAttribute`, gateway event args.
- `DiscoSdk.Hosting/Gateway/**`, `DiscordClient`, `DiscordClientConfig`.
- Packages: `System.IO.Pipelines`, `Microsoft.Extensions.Logging.Abstractions`.

## 3. Models and contracts
- `SendGatewayMessage(op, d)` and `ReceivedGatewayMessage(op, d, s, t)`.
- `ReadyPayload`: `session_id`, `resume_gateway_url`, `shard`, `guilds` (unavailable), `application`.
- `SessionStartLimit`: `total`, `remaining`, `reset_after`, `max_concurrency`.
- `DiscordGatewayUri(url, version, compress)` builds `?v=10&encoding=json[&compress=zlib-stream]`.

## 4. Components
| Type | Responsibility |
|---|---|
| `ShardPool` / `IShardPool` | Creates the shards, applies `max_concurrency` to `IdentifyGate`, starts and stops all shards. |
| `Shard` | Handshake, heartbeat loop with jitter, zombie detection, Identify/Resume, reconnect with capped exponential backoff (≤ 900 s, ±`ReconnectBackoffJitter`), close-code classification. |
| `IdentifyGate` | Counting gate sized to `max_concurrency`. A permit is taken before Identify and released on READY/RESUMED, or on a drop before READY. Discord's 5 s per-bucket window and the `shard_id % max_concurrency` key are not modelled (GW-F16). |
| `DefaultGatewaySocket` / `IGatewaySocket` | `ClientWebSocket` wrapper: User-Agent, HELLO timeout, bounded close handshake (`CloseTimeout`), outbound token bucket (110 + 10 heartbeat tokens per 60 s). |
| `GatewayDecompressFactory` → `GatewayZlibDecompress` / `GatewayNoDecompress` | Transport decompression. zlib uses a single inflater per connection. |
| `GatewayExceptions` | `IsFatalCloseCode` (4004, 4010–4014) and `IsRecoverableTransport`. |
| `ShardEventDispatcher` | Per-shard bounded `Channel<ReceivedGatewayMessage>` (`EventProcessorQueueCapacity`). |
| `MemberChunkCoordinator`, `BufferingMemberChunkSink`, `StreamingMemberChunkSink` | Correlate `GUILD_MEMBERS_CHUNK` replies with op 8 requests by nonce. |

## 5. Public API
- `DiscordClientBuilder.WithIntents`, `WithTotalShards`, `WithGatewayCompressMode`, `WithAutoReconnect`,
  `WithMaxReconnectAttempts`, `WithCloseTimeout`, `WithGatewayUserAgent`, `WithEventProcessorQueueCapacity`.
- `IDiscordClient.StartAsync`, `StopAsync`, `ReconnectAsync`, `WaitReadyAsync`, `WaitShutdownAsync`, `Ping()`.
- Events: `IDiscordClient.GatewayDisconnected` (`Shard`, `Exception`, `WillReconnect`) and `GatewayReconnecting`.
- Commands: `IDiscordClient.UpdatePresence()` (op 3), `IGuildMembers.Request()` (op 8).

## 6. Discord surface
- REST: `GET /gateway`, `GET /gateway/bot`.
- Sent: op 1, 2, 3, 6, 8. Received: op 0, 1, 7, 9, 10, 11. Not used: op 4, 31, 43 (PRD-API-028, PRD-API-024).
- Intents: all 21 documented flags. `GuildMembers`, `GuildPresences` and `MessageContent` are privileged.

## 7. Flows
**Start**
1. `GET /gateway/bot` → recommended shard count and `SessionStartLimit`.
2. The pool creates N shards, and each shard connects.
3. HELLO arrives: schedule the first heartbeat at `interval × random(0..HeartbeatJitter)`.
4. Acquire an `IdentifyGate` permit, then Identify.
5. READY: store `session_id` and `resume_gateway_url`, and set the status to `Ready`.

**Drop**
1. The socket closes or the ACK is missed; the socket is closed with 4900 ("transport fault").
2. The close code is classified:
   - fatal → `OnFatalAsync`, then `GatewayDisconnected(WillReconnect=false)`;
   - otherwise, with a session → Resume on `resume_gateway_url`;
   - otherwise → Identify.
3. On op 9: when `d=false`, drop the session. Close the socket, and the shared retry path then chooses Resume or Identify.

## 8. Concurrency and lifecycle
- Each shard runs its own receive loop, heartbeat timer and dispatch worker. The shards share only the `IdentifyGate` and the REST client.
- `StopAsync` cancels the shard's `CancellationTokenSource`, closes with 1000 within `CloseTimeout`, then force-disposes the socket.
- The dispatch queue is bounded. When it is full, the receive loop awaits, which applies backpressure to the socket.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| No HELLO within `HelloTimeout` | The connection is treated as dead and retried with backoff. |
| Missed heartbeat ACK | Zombie: close with 4900, then Resume. |
| Close 4004 / 4010–4014 | Fatal: no retry, and the bot author is notified. |
| Close 4007 / 4009 | **Current:** Resume is tried, then INVALID_SESSION leads to Identify. **Expected:** Identify directly. |
| `MaxReconnectAttempts` exhausted | Fatal path with the last exception. |
| `AutoReconnect = false` | The shard stays `ConnectionLost`, and the bot author calls `ReconnectAsync`. |
| Outbound burst of more than 110 commands per minute | Commands wait for tokens; heartbeats use the reserved pool of 10. |

## 10. Observability
- Metrics: heartbeat latency, gateway events received, reconnects (SPEC-SDK-05).
- Logs: shard state transitions (`Information`), close codes (`Warning`), fatal (`Error`), plus a
  privileged-intent reminder at start-up.

## 11. Tests
| Test class | Covers |
|---|---|
| `ShardIdentifyFlowTests`, `ShardStartGuardTests` | GW-F01, GW-F05, GW-F52 |
| `ShardHeartbeatTests`, `ShardHeartbeatLatencyMetricTests` | GW-F03, GW-F04, GW-F51, GW-F60, GW-N04 |
| `ShardReconnectTests`, `ShardTransientRetryTests` | GW-F07, GW-F08, GW-F55, GW-F56, GW-F58, GW-F63 – GW-F76 |
| `ShardAutoReconnectTests`, `GatewayDisconnectedEventTests` | GW-F08, GW-F09 |
| `ShardFatalErrorTests`, `WaitShutdownFatalTests` | GW-F09, GW-F67, GW-F72 – GW-F76 |
| `ShardStopAsyncTests` | GW-F23, GW-N03 |
| `IdentifyGateTests`, `ShardIdentifyGateTests` | GW-F16, GW-F17 |
| `GatewayZlibDecompressTests`, `GatewayDecompressTests`, `GatewayDecompressFactoryTests`, `GatewayNoDecompressTests` | GW-F11, GW-N01 |
| `DiscordGatewayClientTests` | GW-F27, GW-F28 |
| `ShardEventDispatcherTests` | GW-F24, GW-F50 |
| `RequestGuildMembersActionTests`, `MemberChunkCoordinatorTests`, `BufferingMemberChunkSinkTests`, `StreamingMemberChunkSinkTests` | GW-F57 |
| `EventHandlerIntentGuardTests`, `PrivilegedIntentReminderTests`, `IntentGuardTests` | GW-F19, GW-F20, GW-F29 – GW-F49 |

Implemented or Partial requirements without a covering test: GW-F02, GW-F10, GW-F15, GW-F18, GW-F21 – GW-F22, GW-F25 – GW-F26, GW-F53 – GW-F54, GW-F59, GW-F61, GW-N02, GW-N05.

## 12. History
| Commit | Change |
|---|---|
| `a9e57d6` | Full WebSocket production-hardening pass (HELLO timeout, close timeout, backoff jitter). |
| `fa4a4e5` | Outbound token bucket (120/60 s) with heartbeat priority. |
| `5ecbc74` | Gateway instruments on `Meter "DiscoSdk"`. |
| `5c9c042` | Registration-time handler intent guard (`RequiresIntentAttribute`). |
| `6322dc5` | Per-shard dispatch worker. |

Next steps:
1. Go straight to Identify on close codes 4007 and 4009.
2. Hold identify permits for 5 s and key them by `shard_id % max_concurrency`.
3. Add initial presence to `DiscordClientBuilder`.
4. Add public commands for op 31 and op 43 (with PRD-API-024 and PRD-API-028).
5. Add a zstd-stream extension point.

## 13. Decisions and rejected alternatives
- **JSON only**: ETF would save little bandwidth compared with zlib and needs a custom codec.
- **Per-shard dispatch queue** rather than one global queue: a slow guild on one shard cannot stall the other shards.
- **Reserved heartbeat tokens**: a burst of presence updates can never starve heartbeats, which would otherwise trigger a zombie reconnect.
