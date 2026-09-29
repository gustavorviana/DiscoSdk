# PRD-API-023 — Stickers

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-023](../specs/api-023-stickers.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/sticker](https://docs.discord.com/developers/resources/sticker) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Stickers are either standard (Nitro packs) or guild-uploaded (PNG, APNG, GIF, Lottie). Bots send stickers in messages, manage guild stickers (multipart upload) and list packs. Guild sticker lists also arrive in `GUILD_CREATE` and `GUILD_STICKERS_UPDATE`.

## 2. Goals
- Guild sticker CRUD with cache-then-REST reads.
- Standard sticker and pack lookup.

## 3. Out of scope
- Sending stickers in messages: [PRD-API-015](api-015-messages.md). Sticker CDN URLs: [PRD-API-001](api-001-http-api-basics.md). Sticker events: [PRD-API-004](api-004-gateway-events.md). Cache policy: [PRD-SDK-04](sdk-04-caching.md).

## 4. Usage scenarios
- As a bot author, I upload a PNG sticker with tags to the guild.
- I look up a standard sticker by id to show its pack.

## 5. Desired developer experience

```csharp
var sticker = await guild.Stickers.Create("wave", "wave", new MessageFile("wave.png", null, bytes)).ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| SK-F01 | Get Sticker | `GET /stickers/{}` | Must | Implemented | `IDiscordClient.GetSticker()` → `StickerClient.GetStickerAsync` |
| SK-F02 | List Sticker Packs | `GET /sticker-packs` | Must | Implemented | `IDiscordClient.GetStickerPacks()` → `StickerClient.ListStickerPacksAsync` |
| SK-F03 | Get Sticker Pack | `GET /sticker-packs/{}` | Should | Missing | — |
| SK-F04 | List Guild Stickers | `GET /guilds/{}/stickers` | Must | Implemented | `IGuildStickers.GetAll()` → `StickerManager` → `StickerClient.ListGuildStickersAsync` |
| SK-F05 | Get Guild Sticker | `GET /guilds/{}/stickers/{}` | Must | Implemented | `IGuildStickers.Get()` → `StickerManager` → `StickerClient.GetGuildStickerAsync` |
| SK-F06 | Create Guild Sticker | `POST /guilds/{}/stickers` | Must | Implemented | `IGuildStickers.Create()` → `StickerClient.CreateGuildStickerAsync` |
| SK-F07 | Modify Guild Sticker | `PATCH /guilds/{}/stickers/{}` | Must | Implemented | `ISticker.Modify()` → `StickerClient.ModifyGuildStickerAsync` |
| SK-F08 | Delete Guild Sticker | `DELETE /guilds/{}/stickers/{}` | Must | Implemented | `ISticker.Delete()` → `StickerClient.DeleteGuildStickerAsync` |
| SK-F09 | Sticker format `PNG` (1) | `sticker-format:1` | Must | Implemented | `StickerFormatType.Png` |
| SK-F10 | Sticker format `APNG` (2) | `sticker-format:2` | Must | Implemented | `StickerFormatType.Apng` |
| SK-F11 | Sticker format `LOTTIE` (3) | `sticker-format:3` | Must | Implemented | `StickerFormatType.Lottie` |
| SK-F12 | Sticker format `GIF` (4) | `sticker-format:4` | Must | Implemented | `StickerFormatType.Gif` |
| SK-F13 | Sticker object (type, format, tags, available, guild, user, sort value) and sticker item | — | Must | Implemented | `ISticker`, `StickerType` |
| SK-F14 | Sticker pack object (stickers, cover, banner, SKU) | — | Should | Implemented | `IStickerPack` |
| SK-F15 | Cache-then-REST reads for guild stickers | — | Should | Implemented | `IGuildStickers.Get`, `IGuildStickers.GetAll` (`StickerFetchMode`) → `StickerManager` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| SK-N01 | Uploads use multipart form fields (`name`, `description`, `tags`, `file`), not JSON. |
| SK-N02 | Sticker mutations accept `WithReason`. |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] Guild sticker CRUD and packs (`StickerClientTests`, `StickerWrapperTests`).
- [x] Cache updates from events (`StickerCacheDispatchTests`).
- [ ] Get Sticker Pack by id (SK-F03).

## 11. Open questions
- None.
