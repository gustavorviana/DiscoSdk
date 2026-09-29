# PRD-API-028 — Voice (roadmap)

| | |
|---|---|
| **Status** | Draft |
| **Spec** | [SPEC-API-028](../specs/api-028-voice.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting`, new `DiscoSdk.Voice` (proposed) |
| **Discord docs** | [resources/voice](https://docs.discord.com/developers/resources/voice), [topics/voice-connections](https://docs.discord.com/developers/topics/voice-connections) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
DiscoSdk has no voice support. Bots cannot join voice or stage channels, play or receive audio, see who is in voice, move or mute members through voice states, or use the soundboard in a call. Voice is four separate subsystems:
1. Voice REST (voice states, regions).
2. The main-gateway voice events and op 4.
3. A second WebSocket per voice session (the voice gateway), including the DAVE end-to-end encryption protocol.
4. UDP/RTP transport with Opus audio and AEAD transport encryption.

This PRD replaces `VOICE_PENDING.md`.

## 2. Goals
- Join, move and leave voice and stage channels, and stream Opus audio with the required transport encryption and DAVE E2EE.
- A voice-state cache (who is in which channel, muted or deafened) fed by `VOICE_STATE_UPDATE`.
- Voice-state REST (`/voice-states`) and region listing.
- Keep the native audio dependencies (Opus, libsodium, MLS) out of the core packages.

## 3. Out of scope
- Audio mixing, transcoding or music-bot features: bot authors bring PCM or Opus.
- Video and Go Live streaming.
- Soundboard REST, which is done (PRD-API-024).

## 4. Usage scenarios
- As a bot author, my bot joins the stage as a speaker, plays a pre-recorded announcement, then leaves.
- A moderation bot moves everyone from an overflowing voice channel to a new one.
- A bot listens for `VOICE_STATE_UPDATE` to log joins and leaves.

**What already works without voice** (from `VOICE_PENDING.md`):
- Stage instance REST and events (PRD-API-022).
- Scheduled events of any entity type (PRD-API-019).
- All non-voice gateway events.

`IGuildStageChannel.RequestToSpeak()` / `CancelRequestToSpeak()` send the REST call, but they have no effect until the bot is in the stage as audience. They are stubs until this PRD ships.

## 5. Desired developer experience

```csharp
// (proposed) DiscoSdk.Voice
await using var connection = await voiceChannel.ConnectAsync(new VoiceConnectOptions { SelfDeaf = true });
await connection.PlayAsync(OpusStream.FromFile("announcement.opus"));
await connection.DisconnectAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| VC-F01 | List Voice Regions | `GET /voice/regions` | Should | Missing | Only the per-guild `IGuild.GetVoiceRegions()` exists (PRD-API-018) |
| VC-F02 | Get Current User Voice State | `GET /guilds/{}/voice-states/@me` | Should | Missing | — |
| VC-F03 | Get User Voice State | `GET /guilds/{}/voice-states/{}` | Should | Missing | — |
| VC-F04 | Modify Current User Voice State | `PATCH /guilds/{}/voice-states/@me` | Must | Partial | `IGuildStageChannel.RequestToSpeak()`, `IGuildStageChannel.CancelRequestToSpeak()` → `GuildClient.RequestToSpeakAsync`; only `request_to_speak_timestamp` is settable (`suppress` and `channel_id` are not), and it has no effect until the bot is in the stage (voice roadmap) |
| VC-F05 | Modify User Voice State | `PATCH /guilds/{}/voice-states/{}` | Should | Missing | — |
| VC-F06 | Voice opcode 0 — Identify | `voice-op:0` | Must | Missing | Voice gateway roadmap |
| VC-F07 | Voice opcode 1 — Select Protocol | `voice-op:1` | Must | Missing | Voice gateway roadmap |
| VC-F08 | Voice opcode 2 — Ready | `voice-op:2` | Must | Missing | Voice gateway roadmap |
| VC-F09 | Voice opcode 3 — Heartbeat | `voice-op:3` | Must | Missing | Voice gateway roadmap |
| VC-F10 | Voice opcode 4 — Session Description | `voice-op:4` | Must | Missing | Voice gateway roadmap |
| VC-F11 | Voice opcode 5 — Speaking | `voice-op:5` | Must | Missing | Voice gateway roadmap |
| VC-F12 | Voice opcode 6 — Heartbeat ACK | `voice-op:6` | Must | Missing | Voice gateway roadmap |
| VC-F13 | Voice opcode 7 — Resume | `voice-op:7` | Must | Missing | Voice gateway roadmap |
| VC-F14 | Voice opcode 8 — Hello | `voice-op:8` | Must | Missing | Voice gateway roadmap |
| VC-F15 | Voice opcode 9 — Resumed | `voice-op:9` | Must | Missing | Voice gateway roadmap |
| VC-F16 | Voice opcode 11 — Clients Connect | `voice-op:11` | Must | Missing | Voice gateway roadmap |
| VC-F17 | Voice opcode 13 — Client Disconnect | `voice-op:13` | Must | Missing | Voice gateway roadmap |
| VC-F18 | Voice opcode 21 — DAVE Prepare Transition | `voice-op:21` | Must | Missing | Voice gateway roadmap |
| VC-F19 | Voice opcode 22 — DAVE Execute Transition | `voice-op:22` | Must | Missing | Voice gateway roadmap |
| VC-F20 | Voice opcode 23 — DAVE Transition Ready | `voice-op:23` | Must | Missing | Voice gateway roadmap |
| VC-F21 | Voice opcode 24 — DAVE Prepare Epoch | `voice-op:24` | Must | Missing | Voice gateway roadmap |
| VC-F22 | Voice opcode 25 — DAVE MLS External Sender | `voice-op:25` | Must | Missing | Voice gateway roadmap |
| VC-F23 | Voice opcode 26 — DAVE MLS Key Package | `voice-op:26` | Must | Missing | Voice gateway roadmap |
| VC-F24 | Voice opcode 27 — DAVE MLS Proposals | `voice-op:27` | Must | Missing | Voice gateway roadmap |
| VC-F25 | Voice opcode 28 — DAVE MLS Commit Welcome | `voice-op:28` | Must | Missing | Voice gateway roadmap |
| VC-F26 | Voice opcode 29 — DAVE MLS Announce Commit Transition | `voice-op:29` | Must | Missing | Voice gateway roadmap |
| VC-F27 | Voice opcode 30 — DAVE MLS Welcome | `voice-op:30` | Must | Missing | Voice gateway roadmap |
| VC-F28 | Voice opcode 31 — DAVE MLS Invalid Commit Welcome | `voice-op:31` | Must | Missing | Voice gateway roadmap |
| VC-F29 | Voice close code 4001 — Unknown opcode | `voice-close:4001` | Must | Missing | Voice gateway roadmap |
| VC-F30 | Voice close code 4002 — Failed to decode payload | `voice-close:4002` | Must | Missing | Voice gateway roadmap |
| VC-F31 | Voice close code 4003 — Not authenticated | `voice-close:4003` | Must | Missing | Voice gateway roadmap |
| VC-F32 | Voice close code 4004 — Authentication failed | `voice-close:4004` | Must | Missing | Voice gateway roadmap |
| VC-F33 | Voice close code 4005 — Already authenticated | `voice-close:4005` | Must | Missing | Voice gateway roadmap |
| VC-F34 | Voice close code 4006 — Session no longer valid | `voice-close:4006` | Must | Missing | Voice gateway roadmap |
| VC-F35 | Voice close code 4009 — Session timeout | `voice-close:4009` | Must | Missing | Voice gateway roadmap |
| VC-F36 | Voice close code 4011 — Server not found | `voice-close:4011` | Must | Missing | Voice gateway roadmap |
| VC-F37 | Voice close code 4012 — Unknown protocol | `voice-close:4012` | Must | Missing | Voice gateway roadmap |
| VC-F38 | Voice close code 4014 — Disconnected | `voice-close:4014` | Must | Missing | Voice gateway roadmap |
| VC-F39 | Voice close code 4015 — Voice server crashed | `voice-close:4015` | Must | Missing | Voice gateway roadmap |
| VC-F40 | Voice close code 4016 — Unknown encryption mode | `voice-close:4016` | Must | Missing | Voice gateway roadmap |
| VC-F41 | Voice close code 4017 — E2EE/DAVE protocol required | `voice-close:4017` | Must | Missing | Voice gateway roadmap |
| VC-F42 | Voice close code 4020 — Bad request | `voice-close:4020` | Must | Missing | Voice gateway roadmap |
| VC-F43 | Voice close code 4021 — Disconnected: Rate Limited | `voice-close:4021` | Must | Missing | Voice gateway roadmap |
| VC-F44 | Voice close code 4022 — Disconnected: Call Terminated | `voice-close:4022` | Must | Missing | Voice gateway roadmap |
| VC-F45 | Update Voice State (op 4) to join, move and leave | — | Must | Missing | `OpCodes.VoiceStateUpdate` is declared but not sent (PRD-API-003, GW-F54) |
| VC-F46 | `VOICE_STATE_UPDATE` / `VOICE_SERVER_UPDATE` handling and voice-state cache | — | Must | Missing | Not dispatched (PRD-API-004, GE-F67, GE-F68). `IGuildVoiceState` is an empty interface, and `IMember.VoiceState` always returns `null` (`GuildMemberWrapper`), so `MemberCachePolicy` voice caching (`VoicePolicy`) never caches anyone |
| VC-F47 | Voice gateway: versioning (v8), identify, select protocol, heartbeat with `seq_ack`, resume, buffered resume | — | Must | Missing | — |
| VC-F48 | IP discovery and UDP session | — | Must | Missing | — |
| VC-F49 | Transport encryption: `aead_aes256_gcm_rtpsize`, `aead_xchacha20_poly1305_rtpsize` | — | Must | Missing | — |
| VC-F50 | Opus RTP packetisation, speaking flags, silence frames | — | Must | Missing | — |
| VC-F51 | DAVE E2EE: MLS group, external sender, key packages, proposals/commits, welcome, transitions, frame encryption | — | Must | Missing | Required for non-stage voice calls |
| VC-F52 | Receiving audio (RTP demux and decrypt) | — | Could | Missing | Not officially supported by Discord |
| VC-F53 | Voice region object | — | Should | Implemented | `IGuild.GetVoiceRegions()` (per-guild list, PRD-API-018) |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| VC-N01 | Audio send loop jitter under 5 ms at 20 ms frames on a single core (no GC allocation per packet). |
| VC-N02 | Native dependencies (libopus, libsodium or .NET `AesGcm`, MLS) ship only in `DiscoSdk.Voice`. |
| VC-N03 | A voice connection survives voice-gateway resumes without audible gaps longer than 200 ms. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Self mute / deaf on join | false / true | bool | (proposed) `VoiceConnectOptions` |
| Encryption mode preference | `aead_aes256_gcm_rtpsize` | supported modes | (proposed) `VoiceConnectOptions.PreferredModes` |

## 9. Compatibility
New package; additive. Filling in `IGuildVoiceState` with members is additive.

## 10. Acceptance criteria
- [ ] Join a voice channel, play 10 s of Opus, and leave cleanly on a test guild (manual script in `TomoriBot`).
- [ ] `VOICE_STATE_UPDATE` populates `IMember.VoiceState`, and `VoicePolicy` caches in-voice members.
- [ ] DAVE handshake passes against Discord for a two-member call.
- [ ] Voice-state REST (VC-F02, VC-F03, VC-F05).

## 11. Open questions
- Opus codec: native libopus (fast, needs binaries per RID) or Concentus (managed, slower)? Provisional: libopus through a small P/Invoke layer, with Concentus as a fallback.
- Transport crypto: .NET `AesGcm` for `aead_aes256_gcm_rtpsize` (no native dependency) and libsodium only for XChaCha20. Provisional: AES-GCM first.
- DAVE/MLS: use Discord's `libdave` through interop, or implement RFC 9420 in managed code? Provisional: `libdave` interop.
- Order of delivery (from `VOICE_PENDING.md`): REST → gateway events and op 4 → voice gateway → UDP/RTP → soundboard in call → DAVE.
