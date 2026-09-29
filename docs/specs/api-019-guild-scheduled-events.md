# SPEC-API-019 — Guild scheduled events

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-019](../prd/api-019-guild-scheduled-events.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/guild-scheduled-event](https://docs.discord.com/developers/resources/guild-scheduled-event) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IGuild.ScheduledEvents` (`GuildScheduledEventsSurface`) lists, gets and creates events. `GuildScheduledEventWrapper` (`IGuildScheduledEvent`) provides `Modify()` (including status) and `GetUsers()`. `GuildScheduledEventClient` implements the six routes.

## 2. Projects and dependencies
`DiscoSdk` (`IGuildScheduledEvent`, `IGuildScheduledEventUser`, `ScheduledEventEntityType`, `ScheduledEventStatus`, `ScheduledEventPrivacyLevel`, the create/modify actions), and `DiscoSdk.Hosting` (client, surface, wrappers).

## 3. Models and contracts
The wire event maps every documented field except `recurrence_rule`.

## 4. Components
| Type | Responsibility |
|---|---|
| `GuildScheduledEventClient` | `ListAsync(with_user_count)`, `CreateAsync`, `GetAsync`, `ModifyAsync`, `DeleteAsync`, `GetUsersAsync(limit, before, after, with_member)`. |
| Create/modify actions | Fluent setters; only modified fields are sent. |

## 5. Public API
`IGuild.ScheduledEvents.GetAll/Get/Create`, `IGuildScheduledEvent.Modify()`, `IGuildScheduledEvent.GetUsers(…)`, `IGuildScheduledEvent.Delete()`.

## 6. Discord surface
Six routes. Events need the `GUILD_SCHEDULED_EVENTS` intent.

## 7. Flows
Start an event: `Modify().SetStatus(Active)` → `PATCH`.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| External event without an end time or location | Discord 400 (not validated locally). |
| Invalid status transition (for example, COMPLETED → ACTIVE) | Discord 400. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `GuildScheduledEventClientTests` | SE-F01 – SE-F06 |
| `GuildScheduledEventWrapperTests` | SE-F10, SE-F11 |
| `GuildScheduledEventDispatchTests` | events (PRD-API-004) |

Implemented or Partial requirements without a covering test: SE-F07 – SE-F09, SE-F13, SE-N01.

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Scheduled events REST. |
| `15c4af6` | `WithReason`. |

Next steps: add recurrence rules, and validate entity-specific fields locally.

## 13. Decisions and rejected alternatives
- **Status changes through `Modify()`**: this mirrors Discord's single PATCH and needs no dedicated methods.
