# SPEC-API-012 — Audit log

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-012](../prd/api-012-audit-log.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/audit-log](https://docs.discord.com/developers/resources/audit-log) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IGuild.GetAuditLogs()` returns an `IAuditLogPaginationAction`. Each page calls
`GuildClient.GetAuditLogsAsync(guildId, limit, before, userId, actionType)` and yields `IAuditLogEntry`
items. Writing reasons is cross-cutting: every mutating action implements `IRestActionWithReason<TSelf>`,
and the REST client adds `X-Audit-Log-Reason` through `SendWithReasonAsync` (SPEC-API-001).

## 2. Projects and dependencies
- `DiscoSdk`: `IAuditLog`, `IAuditLogEntry`, `IAuditLogChange`, `IAuditLogOptions`, `AuditLogActionType`,
  `IAuditLogPaginationAction`, `AuditLogReason`, `IRestActionWithReason`.
- `DiscoSdk.Hosting`: `GuildClient.GetAuditLogsAsync`, `AuditLog` model, pagination action.

## 3. Models and contracts
The wire `AuditLog` keeps only `audit_log_entries`. `users`, `webhooks`, `threads`, `integrations`,
`application_commands`, `auto_moderation_rules` and `guild_scheduled_events` are dropped (AL-F74).

## 4. Components
| Type | Responsibility |
|---|---|
| Audit log pagination action | Uses `before` from the last entry of each page; filters `user_id` and `action_type`. |
| `AuditLogReason` | Header name and 512-character limit. |
| `DiscordRestClient.BuildFactoryWithReason` | Truncate, then URL-encode, then add the header. |

## 5. Public API
`IGuild.GetAuditLogs()` with `SetUserId`, `SetActionType` and `Before`, plus `WithReason(string)` on every
auditable action.

## 6. Discord surface
`GET /guilds/{id}/audit-logs`. Requires `VIEW_AUDIT_LOG`.

## 7. Flows
1. Page 1: `GET ?limit=100&user_id&action_type`.
2. Page N: `before = last.Id`.
3. Stop when a page has fewer entries than the limit.

## 8. Concurrency and lifecycle
Pagination is sequential and lazy.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Missing `VIEW_AUDIT_LOG` | `InsufficientPermissionException`. |
| Unknown action type value | Parsed as an unnamed `AuditLogActionType` value. |

## 10. Observability
REST metrics only.

## 11. Tests
| Test class | Covers |
|---|---|
| `GuildClientTests` | AL-F01, AL-F75 |
| `AuditLogReasonTests`, `AuditLogReasonHeaderTests` | AL-F76, AL-N02 |

## 12. History
| Commit | Change |
|---|---|
| `15c4af6` | `X-Audit-Log-Reason` via `WithReason` on every auditable mutation. |
| `13054cd` | Edit/Delete `WithReason` normalised across `IEmoji` and `IStageInstance`. |

Next steps: add the 15 missing action types, expose related objects, and add the `after` filter.

## 13. Decisions and rejected alternatives
- **Reason as a fluent builder step** rather than an optional parameter on every method: this keeps the
  signatures stable and adds the capability uniformly.
