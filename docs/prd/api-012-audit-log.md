# PRD-API-012 — Audit log

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-012](../specs/api-012-audit-log.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/audit-log](https://docs.discord.com/developers/resources/audit-log) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Moderation bots read the audit log to find out who did what: kicks, bans, role changes, message deletions.
They also write reasons so that their own actions are traceable. The log is paginated, filterable, and uses
about 70 event types with type-specific change keys and options.

## 2. Goals
- A paginated, filterable reader with typed entries, changes and options.
- An `X-Audit-Log-Reason` on every auditable mutation, through one fluent `WithReason`.
- An enum value for every audit log event type.

## 3. Out of scope
- The `GUILD_AUDIT_LOG_ENTRY_CREATE` gateway event: [PRD-API-004](api-004-gateway-events.md).

## 4. Usage scenarios
- As a bot author, when a message is deleted I query `MESSAGE_DELETE` entries to learn who deleted it.
- Every ban my bot issues carries "Banned by /ban (invoker: @mod)" in the audit log.

## 5. Desired developer experience

```csharp
var entries = guild.GetAuditLogs()
    .SetActionType(AuditLogActionType.MemberBanAdd)
    .SetUserId(moderatorId);

await guild.Bans.Ban(userId).WithReason("Spam (invoker: @mod)").ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| AL-F01 | Get Guild Audit Log | `GET /guilds/{}/audit-logs` | Must | Implemented | `IGuild.GetAuditLogs()` → `IAuditLogPaginationAction` → `GuildClient.GetAuditLogsAsync` |
| AL-F02 | Audit log event `GUILD_UPDATE` (1) | `audit:1` | Should | Implemented | `AuditLogActionType.GuildUpdate` |
| AL-F03 | Audit log event `CHANNEL_CREATE` (10) | `audit:10` | Should | Implemented | `AuditLogActionType.ChannelCreate` |
| AL-F04 | Audit log event `CHANNEL_UPDATE` (11) | `audit:11` | Should | Implemented | `AuditLogActionType.ChannelUpdate` |
| AL-F05 | Audit log event `CHANNEL_DELETE` (12) | `audit:12` | Should | Implemented | `AuditLogActionType.ChannelDelete` |
| AL-F06 | Audit log event `CHANNEL_OVERWRITE_CREATE` (13) | `audit:13` | Should | Implemented | `AuditLogActionType.ChannelOverwriteCreate` |
| AL-F07 | Audit log event `CHANNEL_OVERWRITE_UPDATE` (14) | `audit:14` | Should | Implemented | `AuditLogActionType.ChannelOverwriteUpdate` |
| AL-F08 | Audit log event `CHANNEL_OVERWRITE_DELETE` (15) | `audit:15` | Should | Implemented | `AuditLogActionType.ChannelOverwriteDelete` |
| AL-F09 | Audit log event `MEMBER_KICK` (20) | `audit:20` | Should | Implemented | `AuditLogActionType.MemberKick` |
| AL-F10 | Audit log event `MEMBER_PRUNE` (21) | `audit:21` | Should | Implemented | `AuditLogActionType.MemberPrune` |
| AL-F11 | Audit log event `MEMBER_BAN_ADD` (22) | `audit:22` | Should | Implemented | `AuditLogActionType.MemberBanAdd` |
| AL-F12 | Audit log event `MEMBER_BAN_REMOVE` (23) | `audit:23` | Should | Implemented | `AuditLogActionType.MemberBanRemove` |
| AL-F13 | Audit log event `MEMBER_UPDATE` (24) | `audit:24` | Should | Implemented | `AuditLogActionType.MemberUpdate` |
| AL-F14 | Audit log event `MEMBER_ROLE_UPDATE` (25) | `audit:25` | Should | Implemented | `AuditLogActionType.MemberRoleUpdate` |
| AL-F15 | Audit log event `MEMBER_MOVE` (26) | `audit:26` | Should | Implemented | `AuditLogActionType.MemberMove` |
| AL-F16 | Audit log event `MEMBER_DISCONNECT` (27) | `audit:27` | Should | Implemented | `AuditLogActionType.MemberDisconnect` |
| AL-F17 | Audit log event `BOT_ADD` (28) | `audit:28` | Should | Implemented | `AuditLogActionType.BotAdd` |
| AL-F18 | Audit log event `ROLE_CREATE` (30) | `audit:30` | Should | Implemented | `AuditLogActionType.RoleCreate` |
| AL-F19 | Audit log event `ROLE_UPDATE` (31) | `audit:31` | Should | Implemented | `AuditLogActionType.RoleUpdate` |
| AL-F20 | Audit log event `ROLE_DELETE` (32) | `audit:32` | Should | Implemented | `AuditLogActionType.RoleDelete` |
| AL-F21 | Audit log event `INVITE_CREATE` (40) | `audit:40` | Should | Implemented | `AuditLogActionType.InviteCreate` |
| AL-F22 | Audit log event `INVITE_UPDATE` (41) | `audit:41` | Should | Implemented | `AuditLogActionType.InviteUpdate` |
| AL-F23 | Audit log event `INVITE_DELETE` (42) | `audit:42` | Should | Implemented | `AuditLogActionType.InviteDelete` |
| AL-F24 | Audit log event `WEBHOOK_CREATE` (50) | `audit:50` | Should | Implemented | `AuditLogActionType.WebhookCreate` |
| AL-F25 | Audit log event `WEBHOOK_UPDATE` (51) | `audit:51` | Should | Implemented | `AuditLogActionType.WebhookUpdate` |
| AL-F26 | Audit log event `WEBHOOK_DELETE` (52) | `audit:52` | Should | Implemented | `AuditLogActionType.WebhookDelete` |
| AL-F27 | Audit log event `EMOJI_CREATE` (60) | `audit:60` | Should | Implemented | `AuditLogActionType.EmojiCreate` |
| AL-F28 | Audit log event `EMOJI_UPDATE` (61) | `audit:61` | Should | Implemented | `AuditLogActionType.EmojiUpdate` |
| AL-F29 | Audit log event `EMOJI_DELETE` (62) | `audit:62` | Should | Implemented | `AuditLogActionType.EmojiDelete` |
| AL-F30 | Audit log event `MESSAGE_DELETE` (72) | `audit:72` | Should | Implemented | `AuditLogActionType.MessageDelete` |
| AL-F31 | Audit log event `MESSAGE_BULK_DELETE` (73) | `audit:73` | Should | Implemented | `AuditLogActionType.MessageBulkDelete` |
| AL-F32 | Audit log event `MESSAGE_PIN` (74) | `audit:74` | Should | Implemented | `AuditLogActionType.MessagePin` |
| AL-F33 | Audit log event `MESSAGE_UNPIN` (75) | `audit:75` | Should | Implemented | `AuditLogActionType.MessageUnpin` |
| AL-F34 | Audit log event `INTEGRATION_CREATE` (80) | `audit:80` | Should | Implemented | `AuditLogActionType.IntegrationCreate` |
| AL-F35 | Audit log event `INTEGRATION_UPDATE` (81) | `audit:81` | Should | Implemented | `AuditLogActionType.IntegrationUpdate` |
| AL-F36 | Audit log event `INTEGRATION_DELETE` (82) | `audit:82` | Should | Implemented | `AuditLogActionType.IntegrationDelete` |
| AL-F37 | Audit log event `STAGE_INSTANCE_CREATE` (83) | `audit:83` | Should | Implemented | `AuditLogActionType.StageInstanceCreate` |
| AL-F38 | Audit log event `STAGE_INSTANCE_UPDATE` (84) | `audit:84` | Should | Implemented | `AuditLogActionType.StageInstanceUpdate` |
| AL-F39 | Audit log event `STAGE_INSTANCE_DELETE` (85) | `audit:85` | Should | Implemented | `AuditLogActionType.StageInstanceDelete` |
| AL-F40 | Audit log event `STICKER_CREATE` (90) | `audit:90` | Should | Implemented | `AuditLogActionType.StickerCreate` |
| AL-F41 | Audit log event `STICKER_UPDATE` (91) | `audit:91` | Should | Implemented | `AuditLogActionType.StickerUpdate` |
| AL-F42 | Audit log event `STICKER_DELETE` (92) | `audit:92` | Should | Implemented | `AuditLogActionType.StickerDelete` |
| AL-F43 | Audit log event `GUILD_SCHEDULED_EVENT_CREATE` (100) | `audit:100` | Should | Implemented | `AuditLogActionType.ScheduledEventCreate` |
| AL-F44 | Audit log event `GUILD_SCHEDULED_EVENT_UPDATE` (101) | `audit:101` | Should | Implemented | `AuditLogActionType.ScheduledEventUpdate` |
| AL-F45 | Audit log event `GUILD_SCHEDULED_EVENT_DELETE` (102) | `audit:102` | Should | Implemented | `AuditLogActionType.ScheduledEventDelete` |
| AL-F46 | Audit log event `THREAD_CREATE` (110) | `audit:110` | Should | Implemented | `AuditLogActionType.ThreadCreate` |
| AL-F47 | Audit log event `THREAD_UPDATE` (111) | `audit:111` | Should | Implemented | `AuditLogActionType.ThreadUpdate` |
| AL-F48 | Audit log event `THREAD_DELETE` (112) | `audit:112` | Should | Implemented | `AuditLogActionType.ThreadDelete` |
| AL-F49 | Audit log event `APPLICATION_COMMAND_PERMISSION_UPDATE` (121) | `audit:121` | Should | Implemented | `AuditLogActionType.ApplicationCommandPermissionUpdate` |
| AL-F50 | Audit log event `SOUNDBOARD_SOUND_CREATE` (130) | `audit:130` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F51 | Audit log event `SOUNDBOARD_SOUND_UPDATE` (131) | `audit:131` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F52 | Audit log event `SOUNDBOARD_SOUND_DELETE` (132) | `audit:132` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F53 | Audit log event `AUTO_MODERATION_RULE_CREATE` (140) | `audit:140` | Should | Implemented | `AuditLogActionType.AutoModerationRuleCreate` |
| AL-F54 | Audit log event `AUTO_MODERATION_RULE_UPDATE` (141) | `audit:141` | Should | Implemented | `AuditLogActionType.AutoModerationRuleUpdate` |
| AL-F55 | Audit log event `AUTO_MODERATION_RULE_DELETE` (142) | `audit:142` | Should | Implemented | `AuditLogActionType.AutoModerationRuleDelete` |
| AL-F56 | Audit log event `AUTO_MODERATION_BLOCK_MESSAGE` (143) | `audit:143` | Should | Implemented | `AuditLogActionType.AutoModerationActionExecution` (value 143; the name differs from Discord's `AUTO_MODERATION_BLOCK_MESSAGE`) |
| AL-F57 | Audit log event `AUTO_MODERATION_FLAG_TO_CHANNEL` (144) | `audit:144` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F58 | Audit log event `AUTO_MODERATION_USER_COMMUNICATION_DISABLED` (145) | `audit:145` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F59 | Audit log event `AUTO_MODERATION_QUARANTINE_USER` (146) | `audit:146` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F60 | Audit log event `CREATOR_MONETIZATION_REQUEST_CREATED` (150) | `audit:150` | Should | Implemented | `AuditLogActionType.CreatorMonetizationRequestCreated` |
| AL-F61 | Audit log event `CREATOR_MONETIZATION_TERMS_ACCEPTED` (151) | `audit:151` | Should | Implemented | `AuditLogActionType.CreatorMonetizationTermsAccepted` |
| AL-F62 | Audit log event `ONBOARDING_PROMPT_CREATE` (163) | `audit:163` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F63 | Audit log event `ONBOARDING_PROMPT_UPDATE` (164) | `audit:164` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F64 | Audit log event `ONBOARDING_PROMPT_DELETE` (165) | `audit:165` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F65 | Audit log event `ONBOARDING_CREATE` (166) | `audit:166` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F66 | Audit log event `ONBOARDING_UPDATE` (167) | `audit:167` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F67 | Audit log event `HOME_SETTINGS_CREATE` (190) | `audit:190` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F68 | Audit log event `HOME_SETTINGS_UPDATE` (191) | `audit:191` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F69 | Audit log event `VOICE_CHANNEL_STATUS_CREATE` (192) | `audit:192` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F70 | Audit log event `VOICE_CHANNEL_STATUS_DELETE` (193) | `audit:193` | Should | Missing | Not in `AuditLogActionType`; entries still parse but the enum value is unnamed |
| AL-F71 | Audit log entry object (target id, changes, user id, id, action type, options, reason) | — | Must | Implemented | `IAuditLogEntry` |
| AL-F72 | Audit log change object and change-key exceptions | — | Should | Implemented | `IAuditLogChange` |
| AL-F73 | Optional audit entry info (channel, count, delete member days, members removed, message id, role name, type, integration type, auto-moderation fields) | — | Should | Implemented | `IAuditLogEntry.Options` (`IAuditLogOptions`) |
| AL-F74 | Related objects in the response (users, webhooks, threads, integrations, application commands, auto-moderation rules, scheduled events) | — | Should | Missing | `IAuditLog` exposes only `AuditLogEntries` |
| AL-F75 | Filters: `user_id`, `action_type`, `before`, `after`, `limit` (1–100) | — | Must | Partial | `IAuditLogPaginationAction.SetUserId`, `IAuditLogPaginationAction.SetActionType`, `IAuditLogPaginationAction.Before`; no `after` |
| AL-F76 | `X-Audit-Log-Reason` header (URL-encoded, ≤ 512 chars) on auditable mutations | — | Must | Implemented | `IRestActionWithReason.WithReason`, `AuditLogReason` → `DiscordRestClient.SendWithReasonAsync` (commit `15c4af6`) |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| AL-N01 | One request per `ExecuteAsync`, with no pre-fetching. Iterating over several pages is left to the caller (`Before`). |
| AL-N02 | Reasons are URL-encoded and truncated to 512 characters before sending. |

## 8. Configuration
None.

## 9. Compatibility
Adding enum values and related-object collections is additive.

## 10. Acceptance criteria
- [x] Reasons are encoded and truncated (`AuditLogReasonTests`, `AuditLogReasonHeaderTests`).
- [x] Audit log filters (`GuildClientTests`).
- [ ] All 69 action types (AL-F50, AL-F51, AL-F52, AL-F57, AL-F58, AL-F59, AL-F62, AL-F63, AL-F64, AL-F65, AL-F66, AL-F67, AL-F68, AL-F69, AL-F70).
- [ ] Related objects are exposed (AL-F74).

## 11. Open questions
- None.
