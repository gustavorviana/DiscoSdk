# PRD-API-021 — Invites

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-021](../specs/api-021-invites.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/invite](https://docs.discord.com/developers/resources/invite) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Invites bring members into guilds, group DMs and activities. Bots resolve invite codes, for example to show a preview before joining or to moderate links. They also delete invites, track them through gateway events, and restrict invites to target users (the newer target-users API).

## 2. Goals
- Resolve an invite with counts, expiration and the scheduled event.
- Delete invites.
- Map the target-users endpoints.

## 3. Out of scope
- Channel invite list and create: [PRD-API-014](api-014-channels.md). Guild invite list: [PRD-API-018](api-018-guilds.md). `INVITE_CREATE` / `INVITE_DELETE`: [PRD-API-004](api-004-gateway-events.md).

## 4. Usage scenarios
- As a bot author, when someone posts `discord.gg/abc`, I resolve it and show the guild name and member count.
- A moderator deletes a leaked invite through my bot.

## 5. Desired developer experience

```csharp
var invite = await client.GetInvite("abc").WithCounts().ExecuteAsync();
await invite.Delete().ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| IV-F01 | Get Invite | `GET /invites/{}` | Must | Implemented | `IDiscordClient.GetInvite()` → `InviteClient.GetAsync` |
| IV-F02 | Delete Invite | `DELETE /invites/{}` | Must | Implemented | `IInvite.Delete()` → `InviteClient.DeleteAsync` |
| IV-F03 | Get Target Users | `GET /invites/{}/target-users` | Could | Missing | — |
| IV-F04 | Add Target User | `PUT /invites/{}/target-users/{}` | Could | Missing | — |
| IV-F05 | Remove Target User | `DELETE /invites/{}/target-users/{}` | Could | Missing | — |
| IV-F06 | Update Target Users | `PUT /invites/{}/target-users` | Could | Missing | — |
| IV-F07 | Bulk-Add Target Users | `POST /invites/{}/target-users/bulk-add` | Could | Missing | — |
| IV-F08 | Bulk-Delete Target Users | `POST /invites/{}/target-users/bulk-delete` | Could | Missing | — |
| IV-F09 | Get Target Users Job Status | `GET /invites/{}/target-users/job-status` | Could | Missing | — |
| IV-F10 | Invite target type `STREAM` (1) | `invite-target:1` | Must | Implemented | `InviteTargetType.Stream` |
| IV-F11 | Invite target type `EMBEDDED_APPLICATION` (2) | `invite-target:2` | Must | Implemented | `InviteTargetType.EmbeddedApplication` |
| IV-F12 | Invite object (type, code, guild, channel, inviter, target type/user/application, counts, expiry, scheduled event, flags) | — | Must | Partial | `IInvite` has code, channel, guild, inviter, target type/user/application, counts and expiry; `type`, `guild_scheduled_event` and `flags` are missing, and `TargetType` is a raw `int?` rather than `InviteTargetType` |
| IV-F13 | Invite metadata (uses, max uses, max age, temporary, created at) | — | Should | Implemented | `IInvite.Uses`, `IInvite.MaxUses`, `IInvite.MaxAge`, `IInvite.Temporary`, `IInvite.CreatedAt` |
| IV-F14 | Invite types (guild, group DM, friend) | — | Should | Missing | No `type` on `IInvite` and no invite-type enum |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| IV-N01 | Invite deletions accept `WithReason`. |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] Get with query flags and delete (`InviteClientTests`, `GetInviteActionTests`, `InviteWrapperTests`).
- [x] Invite events (`InviteDispatchTests`).
- [ ] Target-users endpoints (IV-F03 – IV-F09).

## 11. Open questions
- None.
