# SPEC-API-008 — Application commands

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-008](../prd/api-008-application-commands.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [interactions/application-commands](https://docs.discord.com/developers/interactions/application-commands) |
| **Last updated** | 2026-09-29 |

## 1. Summary
Commands are modelled as `ApplicationCommand`, a wire-compatible DTO in `DiscoSdk`, with value equality.
They are built by fluent builders (`SlashCommandBuilder`, `ContextMenuBuilder`) or by attribute scanners
(SPEC-SDK-03). During start-up, `DiscordClient` opens an update window:
1. `CommandAutoRegisterModule` adds the discovered commands to per-scope `CommandUpdateScope` instances.
2. `CommandsUpdateWindowOpened` lets the bot author add or remove commands.
3. Each scope's `ApplyAsync` runs, either as one bulk overwrite (`PUT`) or as a diff (`GET` + `POST`/`PATCH`).

Permissions v2 is exposed per guild through `IGuild.Commands` (`IGuildCommands`).

## 2. Projects and dependencies
- `DiscoSdk/Commands/*`: builders, attributes, `ICommandRegistry`.
- `DiscoSdk/Models/Commands/*`: `ApplicationCommand`, options, choices, permissions.
- `DiscoSdk/Rest/Actions/ICommandUpdate*`.
- `DiscoSdk.Hosting/Rest/Clients/ApplicationCommandClient.cs`, `Rest/Actions/CommandUpdateScope.cs`,
  `CommandUpdateFactory.cs`, `Commands/CommandAutoRegisterModule.cs`, `Surfaces/GuildCommandsSurface.cs`.

## 3. Models and contracts
- `ApplicationCommand`: `Id?`, `Type`, `ApplicationId`, `GuildId?`, `Name`, `NameLocalizations`,
  `Description`, `DescriptionLocalizations`, `Options`, `DefaultMemberPermissions`, `DmPermission`,
  `Nsfw`, `IntegrationTypes`, `Contexts`, `Version`. `Equals` compares only the user-controlled fields.
- `ApplicationCommandPermissions`: `{ id, application_id, guild_id, permissions[] }`, where each entry is
  `{ id, type (ROLE/USER/CHANNEL), permission }`.

## 4. Components
| Type | Responsibility |
|---|---|
| `ApplicationCommandClient` | All 16 routes; `EditCommandPermissionsAsync` takes a bearer token. |
| `CommandUpdateFactory` (`ICommandUpdateFactory`) | `OpenForGlobal(overwrite)` / `OpenForGuild(id, overwrite)`: creates independent scopes (used by `[OnDemand]`). |
| `CommandUpdateSession` (`ICommandUpdateSession`) | The update window's session. It caches one scope for global and one per guild, so repeated `Open…` calls in the window merge into the same scope. |
| `CommandUpdateScope` (`ICommandUpdateScope`) | Accumulates commands, applies localizations (`ApplyLocalizations`), then `ApplyAsync` (overwrite → `RegisterAllAsync`; upsert → `GetAllAsync`, then create or edit only what differs). |
| `GuildCommandsSurface` (`IGuildCommands`) | `GetAllPermissions`, `GetPermissions`, `EditPermissions`. |

## 5. Public API
- `IDiscordClient.CommandsUpdateWindowOpened` (`ICommandUpdateSession`), and `ICommandUpdateFactory` from
  dependency injection for on-demand guild registration (`[OnDemand]`).
- `IGuild.Commands`: permissions.
- `DiscordClientBuilder.WithSlashCommands` / `WithContextMenuCommands` for discovery.

## 6. Discord surface
- Routes under `/applications/{id}/commands` and `/applications/{id}/guilds/{id}/commands`.
- The Batch Edit Permissions route is disabled by Discord and not used.
- Command types 1–3 and option types 1–11 are supported. Type 4 (`PRIMARY_ENTRY_POINT`) is not.

## 7. Flows
**Upsert scope**
1. `ApplyLocalizations()`.
2. `GET` the existing commands and index them by name, case-insensitively.
3. For each desired command:
   - not present → `POST`;
   - present and `Equals` → skip;
   - otherwise → `PATCH` by id. If Discord returned no id, log a `Warning` and skip.
4. Commands that exist on Discord but are not desired are left untouched in upsert mode.

**Overwrite scope**
`PUT` the complete list, which removes every command not in it.

## 8. Concurrency and lifecycle
Scopes are applied sequentially within the update window, before `OnReady` handlers that depend on the
commands. `[OnDemand]` guild registration can open additional scopes later.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Name violates Discord rules | `ArgumentException` from the builder, before any request. |
| 100 global commands limit exceeded | Discord 400 → `InvalidRequestBodyException` with field paths. |
| The window opens the same guild twice | `CommandUpdateSession` returns the cached scope, so commands merge. |
| Edit permissions without a bearer token | Discord 401 → `InvalidTokenException`. |

## 10. Observability
A missing id on an existing command is logged at `Warning`. REST calls are covered by the REST metrics (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `SlashCommandValidationTests`, `FluentSlashCommandBuilderTests`, `ContextMenuBuilderTests` | AC-F01, AC-F03 – AC-F07, AC-N03 |
| `SlashCommandAttributeContextsTests`, `SlashCommandGroupingTests` | AC-F04, AC-F06 |
| `CommandLocalizationBuilderTests`, `SlashCommandLocalizerTests`, `ContextCommandLocalizerTests` | AC-F02 |
| `CommandUpdateScopeTests`, `CommandUpdateFactoryTests`, `CommandUpdateScopeAddFromCatalogTests`, `OnDemandRegistrationTests` | AC-F09 – AC-F13, AC-F15, AC-F17 – AC-F19, AC-F21, AC-F23, AC-N01, AC-N02 |
| `ApplicationCommandClientTests` | AC-F12 – AC-F23 (client level) |
| `ApplicationCommandPermissionsClientTests`, `EditApplicationCommandPermissionsActionTests`, `ApplicationCommandPermissionsWrapperTests` | AC-F24 – AC-F26 |
| `ApplicationCommandCountMapConverterTests`, `OptionValueConverterTests` | AC-F32 – AC-F42 |

## 12. History
| Commit | Change |
|---|---|
| `bb4eba3` | Application command permissions REST surface. |
| `649a310` | Command registration overhaul: scanners, frozen registry, session pipeline. |
| `a2a3741`, `6236078` | Slash and context-menu localization providers. |
| `003f9eb` | Unified context-menu attribute, install and use contexts. |
| `bb0ba16` | `IntegrationTypes` / `Contexts` support. |

Next steps:
1. Add `ApplicationCommandType.PrimaryEntryPoint` and a handler-type field.
2. Add a public imperative CRUD surface (PRD §11).

## 13. Decisions and rejected alternatives
- **Declarative scopes with diffing** rather than imperative CRUD: this keeps start-up idempotent and
  avoids re-creating commands (and hitting the per-guild daily create limit) on every deploy.
