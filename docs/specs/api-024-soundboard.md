# SPEC-API-024 — Soundboard

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-024](../prd/api-024-soundboard.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/soundboard](https://docs.discord.com/developers/resources/soundboard) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IGuild.Soundboard` (`GuildSoundboardSurface`) exposes `GetAll`, `Get` and `Create(name, DiscordSoundBuffer)`. `SoundboardSoundWrapper` (`ISoundboardSound`) provides `Modify()` and `Delete()`. `SoundboardSoundClient` implements the five guild routes. Sending a sound and listing default sounds are not implemented, and soundboard gateway events are not dispatched.

## 2. Projects and dependencies
`DiscoSdk` (`ISoundboardSound`, `IGuildSoundboard`, `DiscordSoundBuffer`, create/modify actions), and `DiscoSdk.Hosting` (`SoundboardSoundClient`, `GuildSoundboardSurface`, `SoundboardSoundWrapper`).

## 3. Models and contracts
The wire sound maps `name`, `sound_id`, `volume`, `emoji_id`, `emoji_name`, `guild_id`, `available` and `user`.

## 4. Components
| Type | Responsibility |
|---|---|
| `SoundboardSoundClient` | List, get, create (JSON with a data URI), modify, delete. |
| `DiscordSoundBuffer` | MP3/OGG detection and `ToDataUri()`. |

## 5. Public API
`IGuild.Soundboard.GetAll/Get/Create`, `ISoundboardSound.Modify()` / `Delete()`.

## 6. Discord surface
Seven routes; `POST /channels/{id}/send-soundboard-sound` and `GET /soundboard-default-sounds` are missing.

## 7. Flows
Create: `DiscordSoundBuffer.LoadFile` → `ToDataUri()` → `POST /guilds/{id}/soundboard-sounds`.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Sound over 512 KiB or 5.2 s | Discord 400. |
| Unknown audio format | `DiscordSoundBuffer` rejects it before sending. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `SoundboardSoundClientTests` | SB-F03 – SB-F07 |
| `SoundboardSoundWrapperTests` | SB-F08 |
| `CreateSoundboardSoundActionTests`, `ModifySoundboardSoundActionTests` | SB-F05, SB-F06, SB-N01 |
| `DiscordSoundBufferTests` | SB-F09 |

## 12. History
| Commit | Change |
|---|---|
| `21572e2` | `IGuildSoundboard` facade and REST actions. |

Next steps: add default sounds, send sound (after voice), and soundboard gateway events plus op 31.

## 13. Decisions and rejected alternatives
- **Audio buffer type mirrors `DiscordImageBuffer`**: uploads look the same across expressions.
