# PRD-API-022 — Stage instances

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-022](../specs/api-022-stage-instances.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/stage-instance](https://docs.discord.com/developers/resources/stage-instance) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
A stage instance holds a live stage's topic and privacy, and can notify members when it starts. Bots that host community events open, update and close stage instances. Joining the stage as audio is a voice feature.

## 2. Goals
- Create, get, modify and delete stage instances from the stage channel, with privacy and start notification.

## 3. Out of scope
- Joining the stage, request-to-speak semantics and audio: [PRD-API-028](api-028-voice.md). Stage events: [PRD-API-004](api-004-gateway-events.md).

## 4. Usage scenarios
- As a bot author, at event time my bot opens the stage with the topic "Q&A" and pings members.

## 5. Desired developer experience

```csharp
var stage = await stageChannel.CreateStageInstance("Q&A")
    .SetSendStartNotification(true)
    .ExecuteAsync();
await stage.Modify().SetTopic("Q&A — round 2").ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| ST-F01 | Create Stage Instance | `POST /stage-instances` | Must | Implemented | `IGuildStageChannel.CreateStageInstance()` → `StageInstanceClient.CreateAsync` |
| ST-F02 | Get Stage Instance | `GET /stage-instances/{}` | Must | Implemented | `IGuildStageChannel.GetStageInstance()` → `StageInstanceClient.GetAsync` |
| ST-F03 | Modify Stage Instance | `PATCH /stage-instances/{}` | Must | Implemented | `IStageInstance.Modify()` → `StageInstanceClient.ModifyAsync` |
| ST-F04 | Delete Stage Instance | `DELETE /stage-instances/{}` | Must | Implemented | `IStageInstance.Delete()` → `StageInstanceClient.DeleteAsync` |
| ST-F05 | Stage instance object (topic, privacy level, discoverable disabled, scheduled event id) | — | Must | Implemented | `IStageInstance`, `StagePrivacyLevel` |
| ST-F06 | Start notification (`send_start_notification`) | — | Should | Implemented | `ICreateStageInstanceAction.SetSendStartNotification` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| ST-N01 | Stage mutations accept `WithReason`. |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] CRUD (`StageInstanceClientTests`, `StageInstanceWrapperTests`).
- [x] Stage events (`StageInstanceDispatchTests`).

## 11. Open questions
- None.
