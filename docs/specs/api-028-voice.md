# SPEC-API-028 — Voice (roadmap)

| | |
|---|---|
| **Status** | Draft |
| **PRD** | [PRD-API-028](../prd/api-028-voice.md) |
| **Projects** | `DiscoSdk` (contracts), `DiscoSdk.Hosting` (main-gateway integration), `DiscoSdk.Voice` (proposed, transport) |
| **Discord docs** | [resources/voice](https://docs.discord.com/developers/resources/voice), [topics/voice-connections](https://docs.discord.com/developers/topics/voice-connections), [topics/opcodes-and-status-codes](https://docs.discord.com/developers/topics/opcodes-and-status-codes) |
| **Last updated** | 2026-09-29 |

## 1. Summary
Proposed architecture:
- **Core, main gateway**:
  - `VOICE_STATE_UPDATE` fills a voice-state cache (`IGuildVoiceState` gets real members) and feeds `IMember.VoiceState`.
  - `VOICE_SERVER_UPDATE` is routed to a pending connection.
  - `IShard.UpdateVoiceStateAsync` (proposed) sends op 4.
- **`DiscoSdk.Voice`**: `VoiceConnection` owns:
  - a voice-gateway WebSocket (v8, JSON plus binary DAVE frames);
  - a UDP socket (IP discovery, RTP);
  - an encryption session (AES-256-GCM or XChaCha20-Poly1305);
  - a DAVE session (MLS through `libdave`);
  - an Opus encoder.

`IGuildVoiceChannel.ConnectAsync()` (proposed) is an extension method from the voice package.

## 2. Projects and dependencies
- `DiscoSdk.Voice` (proposed) references `DiscoSdk.Hosting`, an Opus binding and a DAVE binding.
- AES-GCM comes from `System.Security.Cryptography`, and XChaCha20 needs libsodium.
- The core packages gain no native dependency.

## 3. Models and contracts
- `IGuildVoiceState` (to be filled): `GuildId`, `ChannelId`, `UserId`, `Member`, `SessionId`, `Deaf`, `Mute`, `SelfDeaf`, `SelfMute`, `SelfStream`, `SelfVideo`, `Suppress`, `RequestToSpeakTimestamp`.
- The wire `VoiceState` already exists and is parsed from `GUILD_CREATE.voice_states`.

## 4. Components
| Type | Responsibility |
|---|---|
| `VoiceStateManager` (proposed) | Cache keyed by guild and user, from `GUILD_CREATE` and `VOICE_STATE_UPDATE`. |
| `VoiceConnection` (proposed) | State machine: request (op 4) → await `VOICE_STATE_UPDATE` + `VOICE_SERVER_UPDATE` → voice gateway identify → ready → IP discovery → select protocol → session description → DAVE handshake → send loop. |
| `VoiceGatewaySocket` (proposed) | Voice heartbeats with `seq_ack`, resume, buffered resume, binary opcodes 21–31. |
| `RtpTransport` (proposed) | Packetisation, sequence/timestamp, AEAD, silence frames. |
| `DaveSession` (proposed) | MLS group, key packages, commits, welcome, transitions, frame E2EE. |

## 5. Public API
(proposed) `IGuildVoiceChannel.ConnectAsync(options)` / `IGuildStageChannel.ConnectAsync(options)` → `IVoiceConnection` with `PlayAsync(Stream opus)`, `SetSpeaking`, `MoveAsync`, `DisconnectAsync`, plus the events `Disconnected` and `Reconnected`.

## 6. Discord surface
Five voice REST routes, op 4, `VOICE_STATE_UPDATE`, `VOICE_SERVER_UPDATE` and `VOICE_CHANNEL_EFFECT_SEND`, 23 voice opcodes and 16 voice close codes.

## 7. Flows
See the `VoiceConnection` state machine above. On a voice-gateway close:
- codes 4006, 4009, 4014, 4015 → resume or reconnect, per Discord's table;
- code 4017 (DAVE required) → upgrade the protocol.

## 8. Concurrency and lifecycle
One send loop per connection on a dedicated timer (20 ms frames). The voice gateway and UDP loops are independent of the main shard, and a shard reconnect does not drop an established voice connection.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| No `VOICE_SERVER_UPDATE` within 10 s after op 4 | Connect fails with a timeout, and the state is cleaned up. |
| Voice close code 4014 (disconnected: kicked or channel deleted) | Raise `Disconnected` and do not reconnect. |
| Unsupported encryption mode | Close code 4016. Choose from the modes the server offers. |

## 10. Observability
(proposed) metrics for voice RTT, packets sent and lost, encoder underruns, and DAVE transitions.

## 11. Tests
| Test class | Covers |
|---|---|
| `VoiceStateManagerTests` (proposed) | VC-F45, VC-F46 |
| `VoiceGatewaySocketTests` (proposed) | VC-F06 – VC-F44, VC-F47 |
| `RtpTransportTests` (proposed) | VC-F48 – VC-F50 |
| `DaveSessionTests` (proposed) | VC-F51 |
| Manual script in `TomoriBot` | end-to-end join, play, leave |

## 12. Implementation plan
| Commit | Change |
|---|---|
| `—` | Not started. Soundboard REST (`21572e2`) and the stage instance REST are prerequisites that already exist. |

Steps (each one compiles and can be a single commit):
1. Fill `IGuildVoiceState`, parse `VOICE_STATE_UPDATE`, and make `IMember.VoiceState` real (this also fixes `VoicePolicy`).
2. Voice REST: regions, voice states get and modify.
3. Op 4 on `IShard` plus correlating `VOICE_SERVER_UPDATE`.
4. Create `DiscoSdk.Voice`: the voice gateway (identify, heartbeat, resume).
5. IP discovery, UDP, AES-GCM transport, Opus send loop.
6. The DAVE session.
7. Playing soundboard sounds in a call and `VOICE_CHANNEL_EFFECT_SEND`.

## 13. Decisions and rejected alternatives
- **Separate package**: bots without voice avoid native codecs and crypto.
- **Voice-state cache in the core, transport in the voice package**: moderation bots need voice states without audio.
