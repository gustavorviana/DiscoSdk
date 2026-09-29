# PRD-API-024 — Soundboard

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-024](../specs/api-024-soundboard.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/soundboard](https://docs.discord.com/developers/resources/soundboard) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Soundboard sounds (MP3/OGG, ≤ 512 KiB, ≤ 5.2 s) are guild expressions that members, and bots, can play in voice channels. Bots manage a guild's sounds, list Discord's default sounds, and play a sound in a voice channel the bot is connected to.

## 2. Goals
- Guild sound CRUD with typed audio buffers.
- Map default sounds and "send soundboard sound".

## 3. Out of scope
- Playing requires a voice connection: [PRD-API-028](api-028-voice.md). Soundboard gateway events and op 31: [PRD-API-004](api-004-gateway-events.md), [PRD-API-003](api-003-gateway-connection.md).

## 4. Usage scenarios
- As a bot author, I upload a "victory" sound to the guild with an emoji.
- When my bot is in a voice channel, it plays that sound on a command.

## 5. Desired developer experience

```csharp
await guild.Soundboard.Create("victory", DiscordSoundBuffer.LoadFile("victory.mp3")).ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| SB-F01 | Send Soundboard Sound | `POST /channels/{}/send-soundboard-sound` | Should | Missing | Needs a voice connection (PRD-API-028) |
| SB-F02 | List Default Soundboard Sounds | `GET /soundboard-default-sounds` | Should | Missing | — |
| SB-F03 | List Guild Soundboard Sounds | `GET /guilds/{}/soundboard-sounds` | Must | Implemented | `IGuildSoundboard.GetAll()` → `SoundboardSoundClient.ListGuildSoundboardSoundsAsync` |
| SB-F04 | Get Guild Soundboard Sound | `GET /guilds/{}/soundboard-sounds/{}` | Must | Implemented | `IGuildSoundboard.Get()` → `SoundboardSoundClient.GetGuildSoundboardSoundAsync` |
| SB-F05 | Create Guild Soundboard Sound | `POST /guilds/{}/soundboard-sounds` | Must | Implemented | `IGuildSoundboard.Create()` → `SoundboardSoundClient.CreateGuildSoundboardSoundAsync` |
| SB-F06 | Modify Guild Soundboard Sound | `PATCH /guilds/{}/soundboard-sounds/{}` | Must | Implemented | `ISoundboardSound.Modify()` → `SoundboardSoundClient.ModifyGuildSoundboardSoundAsync` |
| SB-F07 | Delete Guild Soundboard Sound | `DELETE /guilds/{}/soundboard-sounds/{}` | Must | Implemented | `ISoundboardSound.Delete()` → `SoundboardSoundClient.DeleteGuildSoundboardSoundAsync` |
| SB-F08 | Soundboard sound object (name, id, volume, emoji, guild, available, user) | — | Must | Implemented | `ISoundboardSound` |
| SB-F09 | Audio upload as a data URI (MP3/OGG detection) | — | Must | Implemented | `DiscordSoundBuffer.ToDataUri` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| SB-N01 | Sound mutations accept `WithReason`. |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] Guild sound CRUD (`SoundboardSoundClientTests`, `SoundboardSoundWrapperTests`, `CreateSoundboardSoundActionTests`, `ModifySoundboardSoundActionTests`, `DiscordSoundBufferTests`).
- [ ] Default sounds and send sound (SB-F01, SB-F02).
- [ ] Soundboard gateway events (PRD-API-004).

## 11. Open questions
- None.
