# PRD-API-008 — Application commands

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-008](../specs/api-008-application-commands.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [interactions/application-commands](https://docs.discord.com/developers/interactions/application-commands) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Slash, user and message commands have to be registered through REST. Registration must be global or per
guild, with localizations, contexts, install types, permissions and option trees. Registering naively on
every start wastes rate limit and causes command flicker. Discord also enforces strict naming and size
rules that fail at runtime.

## 2. Goals
- Declarative registration: the SDK diffs the desired commands against Discord and issues the minimum
  REST calls, or a single bulk overwrite.
- Every command field and option type is modelled and validated before sending.
- Per-command permissions (Permissions v2) can be read and edited.

## 3. Out of scope
- How command classes are discovered, routed and invoked (attributes, scanners, parameter binding):
  [PRD-SDK-03](sdk-03-command-framework.md).
- Receiving and responding to command interactions: [PRD-API-009](api-009-interactions.md).

## 4. Usage scenarios
- As a bot author, I declare `[SlashCommand("status")]` on a class. On start the SDK registers or updates it
  only if it changed.
- I push beta commands to one test guild with a full overwrite, and keep global commands in upsert mode.
- A moderator restricts `/ban` to a role. My dashboard reads and edits that command's permissions with the
  guild admin's bearer token.

## 5. Desired developer experience

```csharp
client.CommandsUpdateWindowOpened += async (_, session) =>
{
    session.OpenForGlobal(overwrite: false)
        .AddSlash(c => c.WithName("ping").WithDescription("Latency check"));
    session.OpenForGuild(testGuildId, overwrite: true)
        .AddSlash(c => c.WithName("debug").WithDescription("Beta only"));
};
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| AC-F01 | Command naming rules (1–32 chars, lowercase, regex) validated client-side | — | Must | Implemented | `SlashCommandBuilder` validation (`SlashCommandValidationTests`) |
| AC-F02 | Name and description localizations | — | Must | Implemented | `ApplicationCommand.NameLocalizations` / `ApplicationCommand.DescriptionLocalizations`, `ICommandLocalizationProvider` (PRD-SDK-03) |
| AC-F03 | `default_member_permissions` | — | Must | Implemented | `SlashCommandBuilder` (`DefaultMemberPermissions`) |
| AC-F04 | Installation contexts (`integration_types`) and interaction contexts (`contexts`) | — | Must | Implemented | `SlashCommandBuilder.WithIntegrationTypes`, `SlashCommandBuilder.WithContexts` |
| AC-F05 | Age-restricted (`nsfw`) commands | — | Should | Implemented | `ApplicationCommand.Nsfw` |
| AC-F06 | Subcommands and subcommand groups | — | Must | Implemented | `SlashCommandSubCommandBuilder`, `SlashCommandSubCommandGroupBuilder`, `[SubCommand]`, `[SubCommandGroup]` |
| AC-F07 | Option constraints: choices, `min/max_value`, `min/max_length`, `channel_types`, `autocomplete`, required | — | Must | Implemented | `SlashCommandStringOptionBuilder`, `SlashCommandIntegerOptionBuilder`, `SlashCommandNumberOptionBuilder`, `SlashCommandChannelOptionBuilder`, `SlashCommandChoiceBuilder` |
| AC-F08 | Entry point commands (`PRIMARY_ENTRY_POINT`, handler types) | — | Could | Missing | No `ApplicationCommandType` value 4 (AC-F31) and no handler-type field |
| AC-F09 | Declarative diff: upsert (create new, patch changed, skip equal) or bulk overwrite | — | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `CommandUpdateScope` |
| AC-F10 | Command update window at start-up (single batch per scope) | — | Must | Implemented | `IDiscordClient.CommandsUpdateWindowOpened` (`ICommandUpdateSession`), `CommandAutoRegisterModule` |
| AC-F11 | Rate-limit awareness: 200 creates per day per guild | — | Should | Implemented | Upsert mode skips unchanged commands, and bulk overwrite is a single request |
| AC-F12 | Get Global Application Commands | `GET /applications/{}/commands` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.GetGlobalCommandsAsync` (read during upsert diff) |
| AC-F13 | Create Global Application Command | `POST /applications/{}/commands` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.CreateGlobalCommandAsync` |
| AC-F14 | Get Global Application Command | `GET /applications/{}/commands/{}` | Should | Partial | `ApplicationCommandClient.GetGlobalCommandAsync` exists but is not reachable from the public API |
| AC-F15 | Edit Global Application Command | `PATCH /applications/{}/commands/{}` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.EditGlobalCommandAsync` |
| AC-F16 | Delete Global Application Command | `DELETE /applications/{}/commands/{}` | Should | Partial | `ApplicationCommandClient.DeleteGlobalCommandAsync` is not reachable publicly; removal only via overwrite mode (`ICommandUpdateSession.OpenForGlobal` with overwrite plus `ICommandUpdateScopeBuilder.Remove`) |
| AC-F17 | Bulk Overwrite Global Application Commands | `PUT /applications/{}/commands` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.RegisterGlobalCommandsAsync` (overwrite mode) |
| AC-F18 | Get Guild Application Commands | `GET /applications/{}/guilds/{}/commands` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.GetGuildCommandsAsync` |
| AC-F19 | Create Guild Application Command | `POST /applications/{}/guilds/{}/commands` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.CreateGuildCommandAsync` |
| AC-F20 | Get Guild Application Command | `GET /applications/{}/guilds/{}/commands/{}` | Should | Partial | `ApplicationCommandClient.GetGuildCommandAsync` exists but is not reachable from the public API |
| AC-F21 | Edit Guild Application Command | `PATCH /applications/{}/guilds/{}/commands/{}` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.EditGuildCommandAsync` |
| AC-F22 | Delete Guild Application Command | `DELETE /applications/{}/guilds/{}/commands/{}` | Should | Partial | `ApplicationCommandClient.DeleteGuildCommandAsync` is not reachable publicly; removal only via overwrite mode |
| AC-F23 | Bulk Overwrite Guild Application Commands | `PUT /applications/{}/guilds/{}/commands` | Must | Implemented | `ICommandUpdateScope.ApplyAsync` → `ApplicationCommandClient.RegisterGuildCommandsAsync` (overwrite mode) |
| AC-F24 | Get Guild Application Command Permissions | `GET /applications/{}/guilds/{}/commands/permissions` | Must | Implemented | `IGuildCommands.GetAllPermissions()` → `ApplicationCommandClient.GetGuildCommandsPermissionsAsync` |
| AC-F25 | Get Application Command Permissions | `GET /applications/{}/guilds/{}/commands/{}/permissions` | Must | Implemented | `IGuildCommands.GetPermissions()` → `ApplicationCommandClient.GetCommandPermissionsAsync` |
| AC-F26 | Edit Application Command Permissions | `PUT /applications/{}/guilds/{}/commands/{}/permissions` | Must | Implemented | `IGuildCommands.EditPermissions()` → `ApplicationCommandClient.EditCommandPermissionsAsync` (Bearer token, as Discord requires) |
| AC-F27 | Batch Edit Application Command Permissions | `PUT /applications/{}/guilds/{}/commands/permissions` | Could | Deprecated | Disabled by Discord with Permissions v2 |
| AC-F28 | Command type `CHAT_INPUT` (1) | `cmdtype:1` | Must | Implemented | `ApplicationCommandType.ChatInput` |
| AC-F29 | Command type `USER` (2) | `cmdtype:2` | Must | Implemented | `ApplicationCommandType.User` |
| AC-F30 | Command type `MESSAGE` (3) | `cmdtype:3` | Must | Implemented | `ApplicationCommandType.Message` |
| AC-F31 | Command type `PRIMARY_ENTRY_POINT` (4) | `cmdtype:4` | Could | Missing | See AC-F08 |
| AC-F32 | Option type `SUB_COMMAND` (1) | `option:1` | Must | Implemented | `SlashCommandOptionType.SubCommand` |
| AC-F33 | Option type `SUB_COMMAND_GROUP` (2) | `option:2` | Must | Implemented | `SlashCommandOptionType.SubCommandGroup` |
| AC-F34 | Option type `STRING` (3) | `option:3` | Must | Implemented | `SlashCommandOptionType.String` |
| AC-F35 | Option type `INTEGER` (4) | `option:4` | Must | Implemented | `SlashCommandOptionType.Integer` |
| AC-F36 | Option type `BOOLEAN` (5) | `option:5` | Must | Implemented | `SlashCommandOptionType.Boolean` |
| AC-F37 | Option type `USER` (6) | `option:6` | Must | Implemented | `SlashCommandOptionType.User` |
| AC-F38 | Option type `CHANNEL` (7) | `option:7` | Must | Implemented | `SlashCommandOptionType.Channel` |
| AC-F39 | Option type `ROLE` (8) | `option:8` | Must | Implemented | `SlashCommandOptionType.Role` |
| AC-F40 | Option type `MENTIONABLE` (9) | `option:9` | Must | Implemented | `SlashCommandOptionType.Mentionable` |
| AC-F41 | Option type `NUMBER` (10) | `option:10` | Must | Implemented | `SlashCommandOptionType.Number` |
| AC-F42 | Option type `ATTACHMENT` (11) | `option:11` | Must | Implemented | `SlashCommandOptionType.Attachment` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| AC-N01 | Registration runs once per start inside the update window; commands are not re-registered on reconnect. |
| AC-N02 | Command equality ignores server-assigned fields (`id`, `version`, `application_id`), so unchanged commands produce no request. |
| AC-N03 | Validation errors are raised before any request (`ArgumentException`), never as a Discord 400. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Attribute scanning | off | assemblies | `DiscordClientBuilder.WithSlashCommands(...)`, `WithContextMenuCommands(...)` |
| Scope mode | upsert | upsert / overwrite | `OpenForGlobal(overwrite)`, `OpenForGuild(id, overwrite)` |
| Localization provider | none | `ICommandLocalizationProvider` | `WithCommandLocalization<T>()` |

## 9. Compatibility
Adding `ApplicationCommandType.PrimaryEntryPoint` is additive. Code that switches exhaustively on
`ApplicationCommandType` should include a default branch.

## 10. Acceptance criteria
- [x] Builders reject invalid names, lengths and option orders (`SlashCommandValidationTests`, `FluentSlashCommandBuilderTests`).
- [x] Upsert creates new commands, patches changed ones and skips equal ones (`CommandUpdateScopeTests`, `CommandUpdateFactoryTests`, `CommandUpdateScopeAddFromCatalogTests`).
- [x] Permissions get, list and edit (`ApplicationCommandPermissionsClientTests`, `EditApplicationCommandPermissionsActionTests`, `ApplicationCommandPermissionsWrapperTests`).
- [ ] Single-command get and delete are reachable from the public API (AC-F14, AC-F16, AC-F20, AC-F22).
- [ ] Entry point commands (AC-F08).

## 11. Open questions
- Expose imperative command CRUD (`IApplicationCommands.Get/Delete`) alongside the declarative scope?
  Provisional: yes, as `IDiscordClient.Commands` for tooling scenarios such as cleanup scripts.
