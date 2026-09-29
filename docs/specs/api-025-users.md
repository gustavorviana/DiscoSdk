# SPEC-API-025 — Users

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-025](../prd/api-025-users.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/user](https://docs.discord.com/developers/resources/user) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IDiscordClient.Me` (`MeSurface`) wraps the `/users/@me` family. `IDiscordClient.GetUser` goes through `UserRepository`, `IDiscordClient.OpenDm` goes through `DmChannelRepository` (with an opened-DM cache), and `IDiscordClient.CreateGroupDm` builds a group DM. `UserClient` implements the routes with the bot token, including the endpoints Discord reserves for OAuth2 bearer tokens (US-F05, US-F08 – US-F10).

## 2. Projects and dependencies
`DiscoSdk` (`IMe`, `IUser`, `IConnection`, `IApplicationRoleConnection`, `IModifyMeAction`, `IGetCurrentGuildsAction`, `UserFlags`, `PremiumType`), and `DiscoSdk.Hosting` (`UserClient`, `MeSurface`, `UserWrapper`, `UserRepository`, `DmChannelRepository`, `CreateGroupDmAction`).

## 3. Models and contracts
The wire `User` maps id, username, discriminator, global name, avatar, bot, system, MFA, banner, accent color, locale, verified, email, flags, premium type, public flags and avatar decoration data. `primary_guild` and `collectibles` are not mapped.

## 4. Components
| Type | Responsibility |
|---|---|
| `UserClient` | Current user get/modify, user get, current guilds (paging and `with_counts`), current guild member, connections, role-connection get/put. |
| `DmChannelRepository` | `POST /users/@me/channels { recipient_id }` with a cache of opened DMs. |
| `CreateGroupDmAction` | `POST /users/@me/channels { access_tokens, nicks }`. |

## 5. Public API
`IDiscordClient.Me.Get/Modify/GetGuilds/GetGuildMember/GetConnections/GetApplicationRoleConnection/UpdateApplicationRoleConnection`, `IDiscordClient.GetUser`, `IDiscordClient.OpenDm`, `IDiscordClient.OpenedDms`, `IDiscordClient.CreateGroupDm`, `IGuild.Leave()`.

## 6. Discord surface
11 routes. The connections, role-connection and current-member routes require OAuth2 bearer scopes (`connections`, `role_connections.write`, `guilds.members.read`).

## 7. Flows
DM: `OpenDm(userId)` → cache hit, or `POST /users/@me/channels` → `IDmChannel` → `SendMessage` (SPEC-API-015).

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| `IMe.GetConnections()` with the bot token | Discord 401 → `InvalidTokenException` (counts toward the invalid-request budget). |
| DM to a user who blocked the bot or shares no guild | Discord 403 (50007) on send. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `UserClientTests` | US-F01 – US-F10 |
| `ModifyMeActionTests`, `GetCurrentGuildsActionTests` | US-F03, US-F04, US-F17 |
| `UserWrapperTests` | US-F12 |
| `OpenedDmsTests`, `DmChannelWrapperTests`, `CreateGroupDmActionTests` | US-F07, US-F16 |

Implemented or Partial requirements without a covering test: US-F14 – US-F15, US-N01.

## 12. History
| Commit | Change |
|---|---|
| `e58c9af` | Create Group DM with `gdm.join` recipients. |
| `84940cc` | `IDiscordClient.OpenedDms` snapshot. |
| `225dad3` | Cohesive surfaces (`IMe`). |

Next steps: add bearer-token overloads or a user client for the OAuth2-scoped endpoints, take `DiscordImageBuffer` in `IModifyMeAction`, and model primary guild and collectibles.

## 13. Decisions and rejected alternatives
- **`IMe` groups the `/users/@me` routes**: one discoverable entry point for the bot's own account.
