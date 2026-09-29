# SPEC-API-020 — Guild templates

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-020](../prd/api-020-guild-templates.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/guild-template](https://docs.discord.com/developers/resources/guild-template) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IGuild.Templates` (`GuildTemplatesSurface`) lists and creates templates. `GuildTemplateWrapper` (`IGuildTemplate`) provides `Sync()`, `Modify()` and `Delete()`, plus `CreateGuild()`, which targets an endpoint Discord no longer documents. `GuildTemplateClient` also hosts the onboarding routes (PRD-API-018).

## 2. Projects and dependencies
`DiscoSdk` (`IGuildTemplate`, `IGuildTemplates`), and `DiscoSdk.Hosting` (`GuildTemplateClient`, `GuildTemplatesSurface`, `GuildTemplateWrapper`).

## 3. Models and contracts
The wire template carries `code`, `name`, `description`, `usage_count`, `creator`, `source_guild_id`, `serialized_source_guild` and `is_dirty`.

## 4. Components
| Type | Responsibility |
|---|---|
| `GuildTemplateClient` | Get by code (not exposed), list, create, sync, modify, delete, and create guild from template (removed). |

## 5. Public API
`IGuild.Templates.GetAll()` / `Create(name, description)`, `IGuildTemplate.Sync()` / `Modify(…)` / `Delete()` / `CreateGuild(…)`.

## 6. Discord surface
Six documented routes plus one removed route (`POST /guilds/templates/{code}`). They require `MANAGE_GUILD`.

## 7. Flows
Sync: `PUT /guilds/{id}/templates/{code}` → the updated template.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| `CreateGuild()` | Fails: the route is no longer served for applications. |
| Template code already exists for the guild (one per guild) | Discord 400. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `GuildTemplateClientTests` | GT-F01 – GT-F06, GT-F08 |
| `GuildTemplateWrapperTests` | GT-F07 |

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Guild templates REST. |
| `4b5467a` | Onboarding inline builder overloads (shared client). |

Next steps: add `IDiscordClient.GetTemplate(code)`, and obsolete `CreateGuild()`.

## 13. Decisions and rejected alternatives
- **Templates and onboarding share `GuildTemplateClient`**: a historical grouping. It should be split when onboarding grows.
