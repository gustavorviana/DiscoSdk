# PRD-API-020 — Guild templates

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-020](../specs/api-020-guild-templates.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/guild-template](https://docs.discord.com/developers/resources/guild-template) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Guild templates snapshot a server's channels, roles and settings under a shareable code. Bots with `MANAGE_GUILD` list, create, sync, modify and delete templates. Creating a guild from a template was removed for bots together with guild creation.

## 2. Goals
- Template CRUD and sync on a guild.
- Look up a template by code.

## 3. Out of scope
- Guild creation (removed by Discord for applications): see [PRD-API-018](api-018-guilds.md).

## 4. Usage scenarios
- As a bot author, after reorganising roles I sync the guild's template so new servers start from the latest layout.

## 5. Desired developer experience

```csharp
var template = await guild.Templates.Create("Base layout").ExecuteAsync();
await template.Sync().ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| GT-F01 | Get Guild Template | `GET /guilds/templates/{}` | Should | Partial | `GuildTemplateClient.GetTemplateAsync` exists, but no public API reaches it (there is no template lookup on `IDiscordClient`) |
| GT-F02 | Get Guild Templates | `GET /guilds/{}/templates` | Must | Implemented | `IGuildTemplates.GetAll()` → `GuildTemplateClient.GetGuildTemplatesAsync` |
| GT-F03 | Create Guild Template | `POST /guilds/{}/templates` | Must | Implemented | `IGuildTemplates.Create()` → `GuildTemplateClient.CreateGuildTemplateAsync` |
| GT-F04 | Sync Guild Template | `PUT /guilds/{}/templates/{}` | Must | Implemented | `IGuildTemplate.Sync()` → `GuildTemplateClient.SyncGuildTemplateAsync` |
| GT-F05 | Modify Guild Template | `PATCH /guilds/{}/templates/{}` | Must | Implemented | `IGuildTemplate.Modify()` → `GuildTemplateClient.ModifyGuildTemplateAsync` |
| GT-F06 | Delete Guild Template | `DELETE /guilds/{}/templates/{}` | Must | Implemented | `IGuildTemplate.Delete()` → `GuildTemplateClient.DeleteGuildTemplateAsync` |
| GT-F07 | Template object (code, name, description, usage count, creator, source guild snapshot, is dirty) | — | Must | Implemented | `IGuildTemplate` |
| GT-F08 | Create guild from template | `POST /guilds/templates/{}` | Should | Deprecated | No longer in Discord's docs at the baseline; guild creation by apps was removed (change log, 2025-04-15). Still exposed as `IGuildTemplate.CreateGuild()` → `GuildTemplateClient.CreateGuildFromTemplateAsync` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| GT-N01 | Template operations require `MANAGE_GUILD`; failures surface as `InsufficientPermissionException`. |

## 8. Configuration
None.

## 9. Compatibility
`IGuildTemplate.CreateGuild()` targets an endpoint Discord removed. Mark it `[Obsolete]` for one minor release, then delete it.

## 10. Acceptance criteria
- [x] Template CRUD and sync (`GuildTemplateClientTests`, `GuildTemplateWrapperTests`).
- [ ] Get template by code is public (GT-F01).
- [ ] `IGuildTemplate.CreateGuild()` is obsoleted (GT-F08).

## 11. Open questions
- None.
