# SPEC-API-017 — Emojis

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-017](../prd/api-017-emojis.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/emoji](https://docs.discord.com/developers/resources/emoji) |
| **Last updated** | 2026-09-29 |

## 1. Summary
Guild emojis are exposed as a cache-backed surface (`IGuild.Emojis` → `GuildEmojisSurface`: `GetCached`, `GetCachedCount`, `Create`) fed by `GUILD_CREATE` and `GUILD_EMOJIS_UPDATE`. Individual emojis (`EmojiWrapper`) provide `Edit()` and `Delete()`. Application emojis live on `IDiscordClient.ApplicationEmojis` (`List`, `Get`, `Create`), and their wrapper (`ApplicationEmojiWrapper`) provides `Edit()` and `Delete()`.

## 2. Projects and dependencies
`DiscoSdk` (`IEmoji`, `Emoji`, `IGuildEmojis`, `IApplicationEmojis`, `ICreateEmojiAction`, `IEditEmojiAction`), and `DiscoSdk.Hosting` (`GuildClient` emoji routes, `ApplicationClient` emoji routes, `GuildEmojisSurface`, `ApplicationEmojisSurface`, the wrappers).

## 3. Models and contracts
`InternalEmoji` (wire) → `IEmoji` (`Id`, `Name`, `Roles`, `User`, `RequireColons`, `Managed`, `Animated`, `Available`).

## 4. Components
| Type | Responsibility |
|---|---|
| `GuildEmojisSurface` | Cache reads plus `Create(name, image)`. There is no REST list or get. |
| `ApplicationEmojisSurface` | REST `List`, `Get` and `Create` for application emojis. |
| `EmojiWrapper` / `ApplicationEmojiWrapper` | `Edit()` / `Delete()` with `WithReason` for guild emojis. |

## 5. Public API
`IGuild.Emojis`, `IDiscordClient.ApplicationEmojis`, `IEmoji.Edit()`, `IEmoji.Delete()`.

## 6. Discord surface
Ten routes; the SDK does not call the guild list and get routes. The routes require `CREATE_GUILD_EXPRESSIONS` or `MANAGE_GUILD_EXPRESSIONS`.

## 7. Flows
Upload: `DiscordImageBuffer` → data URI → `POST /guilds/{id}/emojis` (or `/applications/{id}/emojis`).

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Image over 256 KiB | Discord 400. |
| Guild emoji not in cache (for example, without the `GUILD_EXPRESSIONS` intent) | Not found; there is no REST fallback today. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `GuildClientTests`, `EmojiWrapperTests` | EM-F03 – EM-F05, EM-F11, EM-N02 |
| `ApplicationEmojiClientTests`, `ApplicationEmojiWrapperTests` | EM-F06 – EM-F10 |
| `GuildExtrasDispatchTests` | EM-F12 |

## 12. History
| Commit | Change |
|---|---|
| `9f81c56` | Interface-first `IEmoji`. |
| `15c4af6` | `WithReason` on emoji mutations. |
| `13054cd` | Edit/Delete `WithReason` normalised on `IEmoji`. |
| `54a5c3e` | `IGuild` split into per-resource facades (`IGuild.Emojis`). |

Next steps: add the REST list and get as a cache-miss fallback, and add role restrictions to the builders.

## 13. Decisions and rejected alternatives
- **Cache-first guild emojis**: Discord pushes the full list on every change, so a REST list is rarely needed.
