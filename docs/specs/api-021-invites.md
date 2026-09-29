# SPEC-API-021 — Invites

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-021](../prd/api-021-invites.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/invite](https://docs.discord.com/developers/resources/invite) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IDiscordClient.GetInvite(code)` returns an `IGetInviteAction` with `WithCounts`, `WithExpiration` and scheduled-event options, and it resolves to `InviteWrapper` (`IInvite`) through `InviteClient.GetAsync`. `IInvite.Delete()` calls `InviteClient.DeleteAsync`. Channel invites (list, create) also live on `InviteClient` (PRD-API-014).

## 2. Projects and dependencies
`DiscoSdk` (`IInvite`, `IGetInviteAction`, `InviteTargetType`), and `DiscoSdk.Hosting` (`InviteClient`, `InviteWrapper`, `GetInviteAction`).

## 3. Models and contracts
The wire invite maps code, guild, channel, inviter, target type, target user and target application, approximate counts, expiry and metadata. `type`, `guild_scheduled_event` and `flags` are not mapped.

## 4. Components
| Type | Responsibility |
|---|---|
| `InviteClient` | `GetAsync(code, with_counts, with_expiration, guild_scheduled_event_id)`, `DeleteAsync`, `CreateAsync`, `GetChannelInvitesAsync`. |
| `GetInviteAction` | Query flags builder. |

## 5. Public API
`IDiscordClient.GetInvite(code)`, `IInvite.Delete()`, and the helpers `IInvite.Url`, `IInvite.IsExpired`, `IInvite.IsMaxedOut`.

## 6. Discord surface
Nine invite routes; the seven target-users routes are not implemented.

## 7. Flows
Resolve: `GET /invites/{code}?with_counts=true` → `InviteWrapper`.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Unknown or expired code | `DiscordResourceNotFoundException` (10006). |
| Deleting without `MANAGE_CHANNELS` / `MANAGE_GUILD` | `InsufficientPermissionException`. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `InviteClientTests`, `GetInviteActionTests` | IV-F01, IV-F02 |
| `InviteWrapperTests` | IV-F10 – IV-F13 |
| `InviteDispatchTests` | invite events (PRD-API-004) |

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Invite routes. |
| `15c4af6` | `WithReason` on deletion. |

Next steps: add the target-users API, and add invite `type`, `flags` and the scheduled event.

## 13. Decisions and rejected alternatives
- **Query flags as builder methods**: this avoids a method overload per flag combination.
