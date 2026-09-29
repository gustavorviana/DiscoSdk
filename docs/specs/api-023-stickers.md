# SPEC-API-023 — Stickers

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-023](../prd/api-023-stickers.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/sticker](https://docs.discord.com/developers/resources/sticker) |
| **Last updated** | 2026-09-29 |

## 1. Summary
Guild stickers go through `IGuild.Stickers` (`IGuildStickers`, implemented by `StickerManager`), with cache-then-REST reads (`StickerFetchMode`) and `Create`. `StickerWrapper` (`ISticker`) provides `Modify()` and `Delete()`. Standard stickers and packs are on `IDiscordClient.GetSticker` / `GetStickerPacks`. `StickerClient` implements the routes, and creation uses multipart form fields.

## 2. Projects and dependencies
`DiscoSdk` (`ISticker`, `IStickerPack`, `IGuildStickers`, `IStickerManager`, `StickerFetchMode`, `StickerType`, `StickerFormatType`), and `DiscoSdk.Hosting` (`StickerClient`, `StickerManager`, `StickerWrapper`, `StickerPackWrapper`).

## 3. Models and contracts
The wire sticker maps every documented field. The sticker pack maps `stickers`, `name`, `sku_id`, `cover_sticker_id`, `description` and `banner_asset_id`.

## 4. Components
| Type | Responsibility |
|---|---|
| `StickerClient` | Standard sticker, packs, guild list/get/create/modify/delete (`SendFormDataAsync` for create). |
| `StickerManager` | Cache seeded from `GUILD_CREATE` and updated by `GUILD_STICKERS_UPDATE`; REST fallback per `StickerFetchMode`. |

## 5. Public API
`IGuild.Stickers.Get/GetAll/GetCached/Create`, `IDiscordClient.Stickers` (cross-guild manager), `IDiscordClient.GetSticker`, `IDiscordClient.GetStickerPacks`, `ISticker.Modify()` / `Delete()`.

## 6. Discord surface
Eight routes; `GET /sticker-packs/{id}` is not implemented. Mutations require `CREATE_GUILD_EXPRESSIONS` or `MANAGE_GUILD_EXPRESSIONS`.

## 7. Flows
Create: multipart `name`, `description`, `tags`, `file` → `POST /guilds/{id}/stickers` → the cache is updated when `GUILD_STICKERS_UPDATE` arrives.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| File over 512 KiB or in the wrong format | Discord 400. |
| `StickerFetchMode.CacheOnly` and not cached | Returns `null`. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `StickerClientTests` | SK-F01, SK-F02, SK-F04 – SK-F08, SK-N01 |
| `StickerWrapperTests` | SK-F09 – SK-F14 |
| `StickerCacheDispatchTests` | SK-F15 |

Implemented or Partial requirements without a covering test: SK-N02.

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Sticker REST. |
| `16acd2d` | Cache-aware sticker manager. |

Next steps: add `IDiscordClient.GetStickerPack(id)`.

## 13. Decisions and rejected alternatives
- **Cache-aware manager**: guild sticker lists come for free from the gateway, so REST is used only on a miss.
