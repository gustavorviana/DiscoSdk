# PRD-API-019 — Guild scheduled events

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-019](../specs/api-019-guild-scheduled-events.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/guild-scheduled-event](https://docs.discord.com/developers/resources/guild-scheduled-event) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Scheduled events announce stage, voice or external happenings with start and end times, a cover image and recurrence rules. Bots create them, update their status (start or complete) and list the interested users.

## 2. Goals
- Full CRUD plus the user list, with typed entity types, status, privacy level and recurrence rules.
- Status transitions follow Discord's rules.

## 3. Out of scope
- Scheduled event gateway events: [PRD-API-004](api-004-gateway-events.md).
- The cover image CDN URL: [PRD-API-001](api-001-http-api-basics.md).

## 4. Usage scenarios
- As a bot author, I create an external event for a meetup with a location and end time.
- At start time my bot sets the status to ACTIVE.

## 5. Desired developer experience

```csharp
await guild.ScheduledEvents
    .Create("Community call", start, ScheduledEventEntityType.External)
    .ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| SE-F01 | List Scheduled Events for Guild | `GET /guilds/{}/scheduled-events` | Must | Implemented | `IGuildScheduledEvents.GetAll()` → `GuildScheduledEventClient.ListAsync` |
| SE-F02 | Create Guild Scheduled Event | `POST /guilds/{}/scheduled-events` | Must | Implemented | `IGuildScheduledEvents.Create()` → `GuildScheduledEventClient.CreateAsync` |
| SE-F03 | Get Guild Scheduled Event | `GET /guilds/{}/scheduled-events/{}` | Must | Implemented | `IGuildScheduledEvents.Get()` → `GuildScheduledEventClient.GetAsync` |
| SE-F04 | Modify Guild Scheduled Event | `PATCH /guilds/{}/scheduled-events/{}` | Must | Implemented | `IGuildScheduledEvent.Modify()` → `GuildScheduledEventClient.ModifyAsync` |
| SE-F05 | Delete Guild Scheduled Event | `DELETE /guilds/{}/scheduled-events/{}` | Must | Implemented | `IGuildScheduledEvent.Delete()` → `GuildScheduledEventClient.DeleteAsync` |
| SE-F06 | Get Guild Scheduled Event Users | `GET /guilds/{}/scheduled-events/{}/users` | Must | Implemented | `IGuildScheduledEvent.GetUsers()` → `GuildScheduledEventClient.GetUsersAsync` |
| SE-F07 | Entity type `STAGE_INSTANCE` (1) | `scheduled-entity:1` | Must | Implemented | `ScheduledEventEntityType.StageInstance` |
| SE-F08 | Entity type `VOICE` (2) | `scheduled-entity:2` | Must | Implemented | `ScheduledEventEntityType.Voice` |
| SE-F09 | Entity type `EXTERNAL` (3) | `scheduled-entity:3` | Must | Implemented | `ScheduledEventEntityType.External` |
| SE-F10 | Scheduled event object (status, privacy, entity metadata, creator, user count, cover image) | — | Must | Implemented | `IGuildScheduledEvent`, `ScheduledEventStatus`, `ScheduledEventPrivacyLevel` |
| SE-F11 | Status transitions (SCHEDULED → ACTIVE → COMPLETED, SCHEDULED → CANCELED) | — | Must | Implemented | `IGuildScheduledEvent.Modify()` (status setter) |
| SE-F12 | Recurrence rules (frequency, interval, by weekday/month, count) | — | Should | Missing | `recurrence_rule` is not modelled |
| SE-F13 | Field requirements by entity type (channel for stage/voice, location and end time for external) | — | Should | Partial | `ICreateScheduledEventAction` validates the name only; Discord rejects missing entity-specific fields with 400 |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| SE-N01 | Event mutations accept `WithReason`. |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] CRUD and user listing (`GuildScheduledEventClientTests`, `GuildScheduledEventWrapperTests`).
- [x] Events dispatch (`GuildScheduledEventDispatchTests`).
- [ ] Recurrence rules (SE-F12).

## 11. Open questions
- None.
