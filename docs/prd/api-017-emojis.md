# PRD-API-017 — Emojis

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-017](../specs/api-017-emojis.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/emoji](https://docs.discord.com/developers/resources/emoji) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Custom emojis exist per guild (up to 50–250, depending on boost tier) and per application (up to 2000, usable everywhere by the bot). Bots upload, rename and delete them, and read the guild's emoji list, which Discord also sends in `GUILD_CREATE` and `GUILD_EMOJIS_UPDATE`.

## 2. Goals
- CRUD for guild and application emojis.
- Cached guild emoji list kept up to date by gateway events, plus a REST fallback.

## 3. Out of scope
- Emoji markup and CDN URLs: [PRD-API-001](api-001-http-api-basics.md).
- `GUILD_EMOJIS_UPDATE`: [PRD-API-004](api-004-gateway-events.md).

## 4. Usage scenarios
- As a bot author, I upload the team's logo as an application emoji and use it in every guild.
- When a moderator renames an emoji, my cached copy updates from the gateway.

## 5. Desired developer experience

```csharp
var emoji = await client.ApplicationEmojis.Create("logo", DiscordImageBuffer.LoadFile("logo.png")).ExecuteAsync();
await emoji.Edit().SetName("brand").ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| EM-F01 | List Guild Emojis | `GET /guilds/{}/emojis` | Should | Missing | No REST fetch; the list is available from the cache via `IGuildEmojis.GetCached()` (EM-F12) |
| EM-F02 | Get Guild Emoji | `GET /guilds/{}/emojis/{}` | Should | Missing | No REST fetch; look up `IGuildEmojis.GetCached()` |
| EM-F03 | Create Guild Emoji | `POST /guilds/{}/emojis` | Must | Implemented | `IGuildEmojis.Create()` → `GuildClient.CreateEmojiAsync` |
| EM-F04 | Modify Guild Emoji | `PATCH /guilds/{}/emojis/{}` | Must | Implemented | `IEmoji.Edit()` → `GuildClient.EditEmojiAsync` |
| EM-F05 | Delete Guild Emoji | `DELETE /guilds/{}/emojis/{}` | Must | Implemented | `IEmoji.Delete()` → `GuildClient.DeleteEmojiAsync` |
| EM-F06 | List Application Emojis | `GET /applications/{}/emojis` | Must | Implemented | `IApplicationEmojis.List()` → `ApplicationClient.ListApplicationEmojisAsync` |
| EM-F07 | Get Application Emoji | `GET /applications/{}/emojis/{}` | Must | Implemented | `IApplicationEmojis.Get()` → `ApplicationClient.GetApplicationEmojiAsync` |
| EM-F08 | Create Application Emoji | `POST /applications/{}/emojis` | Must | Implemented | `IApplicationEmojis.Create()` → `ApplicationClient.CreateApplicationEmojiAsync` |
| EM-F09 | Modify Application Emoji | `PATCH /applications/{}/emojis/{}` | Must | Implemented | `IEmoji.Edit()` → `ApplicationEmojiWrapper` → `ApplicationClient.ModifyApplicationEmojiAsync` |
| EM-F10 | Delete Application Emoji | `DELETE /applications/{}/emojis/{}` | Must | Implemented | `IEmoji.Delete()` → `ApplicationEmojiWrapper` → `ApplicationClient.DeleteApplicationEmojiAsync` |
| EM-F11 | Emoji object (id, name, roles, user, require colons, managed, animated, available) | — | Must | Implemented | `IEmoji`, `Emoji` |
| EM-F12 | Guild emoji list kept from `GUILD_CREATE` / `GUILD_EMOJIS_UPDATE` | — | Must | Implemented | `IGuildEmojis.GetCached()`, `IGuildEmojis.GetCachedCount()` |
| EM-F13 | Role-restricted emojis (`roles` on create/modify) | — | Could | Partial | `IEmoji` exposes roles; the create/edit builders do not set them |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| EM-N01 | The image type is detected from magic bytes (`DiscordImageBuffer`). Discord enforces the 256 KiB size limit (400 on violation). |
| EM-N02 | Emoji mutations accept `WithReason`. |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] Guild emoji create/edit/delete (`GuildClientTests`, `EmojiWrapperTests`).
- [x] Application emoji CRUD (`ApplicationEmojiClientTests`, `ApplicationEmojiWrapperTests`).
- [ ] REST list/get of guild emojis (EM-F01, EM-F02).

## 11. Open questions
- None.
