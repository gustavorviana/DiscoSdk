# PRD-API-025 — Users

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-025](../specs/api-025-users.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/user](https://docs.discord.com/developers/resources/user) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
The user resource covers the bot's own account (profile, avatar, banner, guild list), other users, DMs, and the OAuth2-scoped endpoints: connections, linked-role connections, and the current user's member object. Those require a user's bearer token, not the bot token. Mixing up the two auth modes silently yields 401s.

## 2. Goals
- Every bot-token user endpoint is reachable from `IMe` / `IDiscordClient`.
- Bearer-token endpoints are exposed through an API that takes an OAuth2 access token.
- The user object is modelled completely (flags, premium, avatar decoration, primary guild, collectibles).

## 3. Out of scope
- OAuth2 token flows: [PRD-API-007](api-007-oauth2-teams.md). User CDN URLs: [PRD-API-001](api-001-http-api-basics.md). `USER_UPDATE`: [PRD-API-004](api-004-gateway-events.md).

## 4. Usage scenarios
- As a bot author, I change the bot's avatar and banner from a deploy script.
- I DM a user a reminder.
- My linked-roles web app updates a user's role-connection metadata with their bearer token.

## 5. Desired developer experience

```csharp
var avatar = DiscordImageBuffer.LoadFile("avatar.png");
await client.Me.Modify()
    .SetAvatar($"data:image/png;base64,{avatar.ToBase64()}")   // raw data URI today, see US-F17
    .ExecuteAsync();
var dm = await client.OpenDm(userId).ExecuteAsync();
await dm.SendMessage("Reminder: standup in 5 minutes").ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| US-F01 | Get Current User | `GET /users/@me` | Must | Implemented | `IMe.Get()` → `UserClient.GetCurrentAsync` |
| US-F02 | Get User | `GET /users/{}` | Must | Implemented | `IDiscordClient.GetUser()` → `UserRepository` → `UserClient.GetAsync` |
| US-F03 | Modify Current User | `PATCH /users/@me` | Must | Implemented | `IMe.Modify()` → `UserClient.ModifyCurrentAsync` |
| US-F04 | Get Current User Guilds | `GET /users/@me/guilds` | Must | Implemented | `IMe.GetGuilds()` → `UserClient.GetCurrentGuildsAsync` |
| US-F05 | Get Current User Guild Member | `GET /users/@me/guilds/{}/member` | Could | Partial | `IMe.GetGuildMember()` → `UserClient.GetCurrentGuildMemberAsync` sends the bot token, but Discord requires a bearer token with `guilds.members.read` |
| US-F06 | Leave Guild | `DELETE /users/@me/guilds/{}` | Must | Implemented | `IGuild.Leave()` → `GuildClient.LeaveAsync` |
| US-F07 | Create DM / Create Group DM | `POST /users/@me/channels` | Must | Implemented | `IDiscordClient.OpenDm()` → `DmChannelRepository` → `ChannelClient.CreateDMAsync`; `IDiscordClient.CreateGroupDm()` → `ChannelClient.CreateGroupDmAsync` (GameBridge-era; access tokens required) |
| US-F08 | Get Current User Connections | `GET /users/@me/connections` | Could | Partial | `IMe.GetConnections()` → `UserClient.GetConnectionsAsync` sends the bot token; Discord requires the `connections` bearer scope |
| US-F09 | Get Current User Application Role Connection | `GET /users/@me/applications/{}/role-connection` | Should | Partial | `IMe.GetApplicationRoleConnection()` sends the bot token; Discord requires a bearer token with `role_connections.write` |
| US-F10 | Update Current User Application Role Connection | `PUT /users/@me/applications/{}/role-connection` | Should | Partial | `IMe.UpdateApplicationRoleConnection()` sends the bot token; Discord requires a bearer token with `role_connections.write` |
| US-F11 | Delete Current User Application Role Connection | `DELETE /users/@me/applications/{}/role-connection` | Should | Missing | — |
| US-F12 | User object (id, username, global name, avatar, bot, system, MFA, banner, accent color, locale, verified, flags, premium type, public flags, avatar decoration) | — | Must | Implemented | `IUser`, `UserFlags`, `PremiumType` |
| US-F13 | User primary guild (guild tag) and collectibles (nameplate) | — | Could | Missing | Not modelled |
| US-F14 | Connection object (type, verified, friend sync, visibility, integrations) | — | Could | Implemented | `IConnection`, `ConnectionVisibility` |
| US-F15 | Application role connection object (platform name, username, metadata) | — | Should | Implemented | `IApplicationRoleConnection` |
| US-F16 | Opened-DM cache | — | Should | Implemented | `IDiscordClient.OpenedDms`, `DmChannelRepository` (commit `84940cc`) |
| US-F17 | Modify current user: username, avatar, banner | — | Must | Partial | `IModifyMeAction.SetAvatar` / `SetBanner` take raw data-URI strings instead of `DiscordImageBuffer` (inconsistent with `IEditGuildAction.SetIcon`) |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| US-N01 | Bearer-token endpoints must never be sent with the bot token (they fail with 401 and count toward the invalid-request budget, PRD-API-002). |

## 8. Configuration
None.

## 9. Compatibility
Adding bearer-token overloads (for example `IMe.GetConnections(string accessToken)`) is additive. The current parameterless overloads should be obsoleted, because they cannot succeed with a bot token.

## 10. Acceptance criteria
- [x] Bot-token endpoints (`UserClientTests`, `ModifyMeActionTests`, `GetCurrentGuildsActionTests`, `UserWrapperTests`).
- [x] DMs (`OpenedDmsTests`, `DmChannelWrapperTests`, `CreateGroupDmActionTests`).
- [ ] Bearer-token variants for connections, role connection and current guild member (US-F05, US-F08 – US-F11).

## 11. Open questions
- Where do bearer-token calls live: overloads on `IMe`, or a separate `DiscordUserClient` (see PRD-API-007 §11)? Provisional: a separate user client.
