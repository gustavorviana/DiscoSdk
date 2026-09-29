# SPEC-API-013 — Auto moderation

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-013](../prd/api-013-auto-moderation.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/auto-moderation](https://docs.discord.com/developers/resources/auto-moderation) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IGuild.AutoModeration` (`GuildAutoModerationSurface`) exposes `GetAll`, `Get` and `Create`. Rules are
wrapped as `AutoModerationRuleWrapper` (`IAutoModerationRule`), which provides `Modify()` and `Delete()`.
`AutoModerationClient` implements the five routes. Trigger and action metadata are plain models whose
nullable fields correspond to each trigger or action type.

## 2. Projects and dependencies
- `DiscoSdk/Models/AutoModeration/*` and the enums `AutoModerationTriggerType`, `AutoModerationEventType`,
  `AutoModerationActionType`, `AutoModerationKeywordPresetType`.
- `DiscoSdk.Hosting`: `AutoModerationClient`, `GuildAutoModerationSurface`, `AutoModerationRuleWrapper`, and the create/modify actions.

## 3. Models and contracts
- `AutoModerationTriggerMetadata`: `keyword_filter`, `regex_patterns`, `presets`, `allow_list`,
  `mention_total_limit`, `mention_raid_protection_enabled`.
- `AutoModerationActionMetadata`: `channel_id`, `duration_seconds`, `custom_message`.

## 4. Components
| Type | Responsibility |
|---|---|
| `AutoModerationClient` | `ListRulesAsync`, `GetRuleAsync`, `CreateRuleAsync`, `ModifyRuleAsync`, `DeleteRuleAsync`, with reasons. |
| `GuildAutoModerationSurface` | Binds the guild id and returns builders. |
| `AutoModerationRuleWrapper` | Rule data plus `Modify()` / `Delete()`. |

## 5. Public API
`IGuild.AutoModeration.GetAll()` / `Get(id)` / `Create(name, eventType, triggerType)`,
`IAutoModerationRule.Modify()` / `Delete()`, and the handlers `IAutoModerationRule*Handler` and
`IAutoModerationActionExecutionHandler`.

## 6. Discord surface
Routes under `/guilds/{id}/auto-moderation/rules`. Requires `MANAGE_GUILD`. The events need the
`AUTO_MODERATION_CONFIGURATION` and `AUTO_MODERATION_EXECUTION` intents.

## 7. Flows
Create: builder → validate the required name, event and trigger → `POST` → wrapped rule.

## 8. Concurrency and lifecycle
Stateless; rules are not cached.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Over the per-guild rule limit for a trigger type | Discord 400 → `InvalidRequestBodyException`. |
| Missing `MANAGE_GUILD` | `InsufficientPermissionException`. |

## 10. Observability
REST metrics only.

## 11. Tests
| Test class | Covers |
|---|---|
| `AutoModerationClientTests`, `AutoModerationRuleWrapperTests` | AM-F01 – AM-F05, AM-F20, AM-N01 |
| `AutoModerationTriggerMetadataTests` | AM-F06 – AM-F12, AM-F17, AM-F18 |
| `AutoModerationActionMetadataTests` | AM-F13 – AM-F16, AM-F19 |
| `AutoModerationDispatchTests` | events (PRD-API-004) |

## 12. History
| Commit | Change |
|---|---|
| `1b3cdfa` | Tests for all trigger and action metadata variants. |

## 13. Decisions and rejected alternatives
- **Metadata as nullable-field models** rather than one class per trigger: this mirrors Discord's single
  object and keeps serialisation trivial.
