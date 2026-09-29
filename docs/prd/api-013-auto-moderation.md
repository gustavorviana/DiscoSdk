# PRD-API-013 — Auto moderation

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-013](../specs/api-013-auto-moderation.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/auto-moderation](https://docs.discord.com/developers/resources/auto-moderation) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Discord's native AutoMod blocks, alerts and times out members based on keyword, spam, preset, mention-spam and
member-profile triggers. Moderation bots configure these rules programmatically, and they react to
`AUTO_MODERATION_ACTION_EXECUTION`. Each trigger type has its own metadata and limits.

## 2. Goals
- Full CRUD for rules, with typed trigger and action metadata for every trigger and action type.
- Rule events and execution events delivered to handlers (see PRD-API-004).

## 3. Out of scope
- The gateway events themselves: [PRD-API-004](api-004-gateway-events.md) (GE-F04 – GE-F07).

## 4. Usage scenarios
- As a bot author, I create a keyword rule that blocks invite links and alerts a mod channel.
- When a rule fires, my handler posts context to the mod log.

## 5. Desired developer experience

```csharp
await guild.AutoModeration
    .Create("No invites", AutoModerationEventType.MessageSend, AutoModerationTriggerType.Keyword)
    .ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| AM-F01 | List Auto Moderation Rules for Guild | `GET /guilds/{}/auto-moderation/rules` | Must | Implemented | `IGuildAutoModeration.GetAll()` → `AutoModerationClient.ListRulesAsync` |
| AM-F02 | Get Auto Moderation Rule | `GET /guilds/{}/auto-moderation/rules/{}` | Must | Implemented | `IGuildAutoModeration.Get()` → `AutoModerationClient.GetRuleAsync` |
| AM-F03 | Create Auto Moderation Rule | `POST /guilds/{}/auto-moderation/rules` | Must | Implemented | `IGuildAutoModeration.Create()` → `AutoModerationClient.CreateRuleAsync` |
| AM-F04 | Modify Auto Moderation Rule | `PATCH /guilds/{}/auto-moderation/rules/{}` | Must | Implemented | `IAutoModerationRule.Modify()` → `AutoModerationClient.ModifyRuleAsync` |
| AM-F05 | Delete Auto Moderation Rule | `DELETE /guilds/{}/auto-moderation/rules/{}` | Must | Implemented | `IAutoModerationRule.Delete()` → `AutoModerationClient.DeleteRuleAsync` |
| AM-F06 | Trigger type `KEYWORD` (1) | `automod-trigger:1` | Must | Implemented | `AutoModerationTriggerType.Keyword` |
| AM-F07 | Trigger type `SPAM` (3) | `automod-trigger:3` | Must | Implemented | `AutoModerationTriggerType.Spam` |
| AM-F08 | Trigger type `KEYWORD_PRESET` (4) | `automod-trigger:4` | Must | Implemented | `AutoModerationTriggerType.KeywordPreset` |
| AM-F09 | Trigger type `MENTION_SPAM` (5) | `automod-trigger:5` | Must | Implemented | `AutoModerationTriggerType.MentionSpam` |
| AM-F10 | Trigger type `MEMBER_PROFILE` (6) | `automod-trigger:6` | Must | Implemented | `AutoModerationTriggerType.MemberProfile` |
| AM-F11 | Event type `MESSAGE_SEND` (1) | `automod-event:1` | Must | Implemented | `AutoModerationEventType.MessageSend` |
| AM-F12 | Event type `MEMBER_UPDATE` (2) | `automod-event:2` | Must | Implemented | `AutoModerationEventType.MemberUpdate` |
| AM-F13 | Action type `BLOCK_MESSAGE` (1) | `automod-action:1` | Must | Implemented | `AutoModerationActionType.BlockMessage` |
| AM-F14 | Action type `SEND_ALERT_MESSAGE` (2) | `automod-action:2` | Must | Implemented | `AutoModerationActionType.SendAlertMessage` |
| AM-F15 | Action type `TIMEOUT` (3) | `automod-action:3` | Must | Implemented | `AutoModerationActionType.Timeout` |
| AM-F16 | Action type `BLOCK_MEMBER_INTERACTION` (4) | `automod-action:4` | Must | Implemented | `AutoModerationActionType.BlockMemberInteraction` |
| AM-F17 | Trigger metadata (keyword filter, regex patterns, presets, allow list, mention total limit, mention raid protection) | — | Must | Implemented | `IAutoModerationTriggerMetadata`, `AutoModerationTriggerMetadata` |
| AM-F18 | Keyword preset types (profanity, sexual content, slurs) | — | Must | Implemented | `AutoModerationKeywordPresetType` |
| AM-F19 | Action metadata (alert channel, timeout duration, custom block message) | — | Must | Implemented | `IAutoModerationActionMetadata`, `AutoModerationActionMetadata` |
| AM-F20 | Exempt roles and channels; enabled flag | — | Must | Implemented | `IAutoModerationRule.ExemptRoles`, `IAutoModerationRule.ExemptChannels`, `IAutoModerationRule.Enabled` |
| AM-F21 | Trigger limits (max rules per trigger type, keyword counts and lengths) validated client-side | — | Could | Missing | Discord validates and returns 400 |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| AM-N01 | Rule mutations accept `WithReason` (audit log). |

## 8. Configuration
None.

## 9. Compatibility
A new trigger or action type is additive, together with its metadata class.

## 10. Acceptance criteria
- [x] Every trigger and action metadata variant round-trips (`AutoModerationTriggerMetadataTests`, `AutoModerationActionMetadataTests`).
- [x] Rule CRUD (`AutoModerationClientTests`, `AutoModerationRuleWrapperTests`).
- [x] Rule and execution events dispatch (`AutoModerationDispatchTests`).

## 11. Open questions
- None.
