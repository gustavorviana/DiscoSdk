# SPEC-API-022 — Stage instances

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-022](../prd/api-022-stage-instances.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/stage-instance](https://docs.discord.com/developers/resources/stage-instance) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IGuildStageChannel.CreateStageInstance(topic)` returns an `ICreateStageInstanceAction` (privacy, start notification, scheduled event). `GetStageInstance()` fetches the live instance. `StageInstanceWrapper` (`IStageInstance`) provides `Modify()` and `Delete()` with reasons. `StageInstanceClient` implements the four routes.

## 2. Projects and dependencies
`DiscoSdk` (`IStageInstance`, `ICreateStageInstanceAction`, `IModifyStageInstanceAction`, `StagePrivacyLevel`), and `DiscoSdk.Hosting` (`StageInstanceClient`, `StageInstanceWrapper`, the actions).

## 3. Models and contracts
The wire stage instance maps `id`, `guild_id`, `channel_id`, `topic`, `privacy_level`, `discoverable_disabled` and `guild_scheduled_event_id`.

## 4. Components
| Type | Responsibility |
|---|---|
| `StageInstanceClient` | `CreateAsync`, `GetAsync(channelId)`, `ModifyAsync(channelId)`, `DeleteAsync(channelId)`, with reasons. |

## 5. Public API
`IGuildStageChannel.CreateStageInstance(topic)` / `GetStageInstance()`, `IStageInstance.Modify()` / `Delete()`.

## 6. Discord surface
Routes `/stage-instances` and `/stage-instances/{channel_id}`. They require `MANAGE_CHANNELS`, `MUTE_MEMBERS` and `MOVE_MEMBERS`, plus `MENTION_EVERYONE` for the start notification.

## 7. Flows
Open: `POST /stage-instances { channel_id, topic, privacy_level, send_start_notification }`.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| An instance already exists for the channel | Discord 400. |
| Missing stage moderator permissions | `InsufficientPermissionException`. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `StageInstanceClientTests` | ST-F01 – ST-F04 |
| `StageInstanceWrapperTests` | ST-F05, ST-F06, ST-N01 |
| `StageInstanceDispatchTests` | stage events (PRD-API-004) |

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Stage instance REST. |
| `13054cd` | Edit/Delete `WithReason` normalised on `IStageInstance`. |


## 13. Decisions and rejected alternatives
- **Stage instance keyed by channel id**: this matches Discord's routes and avoids tracking a separate instance id.
