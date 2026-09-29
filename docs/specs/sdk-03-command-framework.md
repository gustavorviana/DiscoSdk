# SPEC-SDK-03 — Command framework

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-SDK-03](../prd/sdk-03-command-framework.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — |
| **Last updated** | 2026-09-29 |

## 1. Summary
At `Build()`, `SlashCommandScanner` and `ContextMenuCommandScanner` reflect over the configured assemblies.
They produce `CommandInfo` (flat commands), `SlashGroupInfo` (subcommands and groups), `ContextMenuCommandInfo`
and `AutoCompleteInfo` entries. The scanners add them to a mutable `CommandRegistryBuilder` and register
each handler class as a scoped service. `Build()` freezes it into
`CommandRegistry`, which holds `FrozenDictionary` / `FrozenSet` indexes. Each entry also carries the
`ApplicationCommand` built through `SlashCommandBuilder` (validated) and localized by
`SlashCommandLocalizer` / `ContextCommandLocalizer`.

At runtime, `SlashCommandDispatcher` and `ContextMenuCommandDispatcher` are internal event handlers
(SPEC-SDK-02). They look up the entry by name (plus subcommand path or command type) and call
`CommandInfo.ExecuteAsync`, which binds parameters and adapts the return value through `MethodCaller`.
Registration with Discord happens in the command update window: `CommandAutoRegisterModule` queues the
entries into the shared `ICommandUpdateSession` (SPEC-API-008).

## 2. Projects and dependencies
- `DiscoSdk/Commands/**`: attributes (`SlashCommandAttribute`, `SlashOptionAttribute`, `SubCommandAttribute`,
  `SubCommandGroupAttribute`, `ChoiceAttribute`, `EnumChoicesAttribute`, `ContextMenuCommandAttribute`,
  `OnDemandAttribute`, `FromServicesAttribute`, `AutoCompleteHandlerAttribute`), handler bases
  (`SlashCommandHandler`, `UserContextMenuHandler`, `MessageContextMenuHandler`), `IAutoComplete`,
  `IParamProvider`, the fluent `SlashCommandBuilder` family and `Localization/*`.
- `DiscoSdk.Hosting/Commands/**`: scanners, `CommandReflection`, `CommandRegistryBuilder`, `CommandRegistry`,
  dispatchers, `Callers/Parameters/*`, `Callers/Results/*`, `Providers/*`, `Localization/*`,
  `CommandAutoRegisterModule`, `ApplicationCommandTypeGuard`.
- Packages: none beyond the base class library (`System.Collections.Frozen`).

## 3. Models and contracts
- `CommandInfo`: the handler type, the `MethodInfo`, the `ParameterCollection` and the `MethodCaller`.
- `SlashEntry(Name, ApplicationCommand, GuildIds, Flat, Group)`: exactly one of `Flat` / `Group` is set.
- `AutoCompleteName`: (command, group, subcommand, option). It is built from the attribute at scan time and
  from `IAutoCompleteContext` at runtime (`AutoCompleteName.FromContext`).
- Parameter kinds (`Callers/Parameters`):
  - `ContextParameterInfo`: the command context;
  - `SlashParamInfo`: an option, converted through `IObjectConverter`;
  - `ServiceParamInfo`: `[FromServices]`, resolved from the scope;
  - `CancellationTokenParamInfo`;
  - `ParamFactoryInfo`: an `IParamProvider` such as `UserParamProvider`, `MemberParamProvider`,
    `ChannelParamProvider` or `GuildParamProvider`.
- Result adapters (`Callers/Results`): `VoidResult`, `TaskResult`, and `RestResult`, which executes a
  returned `IRestAction`.

## 4. Components
| Type | Responsibility |
|---|---|
| `SlashCommandScanner` | Finds `SlashCommandHandler` subclasses and their attributed methods; groups subcommands; collects choices, constraints and autocomplete bindings. |
| `ContextMenuCommandScanner` | Finds `UserContextMenuHandler` / `MessageContextMenuHandler` subclasses with `ContextMenuCommandAttribute`. |
| `CommandReflection` | Shared attribute and parameter reflection helpers. |
| `CommandRegistryBuilder` | Mutable accumulation; rejects duplicate names (a flat command and a group cannot share a name); `Build()` is idempotent and locks the builder. |
| `CommandRegistry` | Frozen lookups (`FindSlash`, `FindAutoComplete`, context menu by (name, type)); `GetAll` / `GetOnDemand` / `IsOnDemand`; auto-register enumeration. Slash names are case-insensitive. |
| `SlashCommandDispatcher` | `IDiscordEventHandler<ICommandContext>` and `IDiscordEventHandler<IAutoCompleteContext>`. |
| `ContextMenuCommandDispatcher` | User and message command routing by (name, type). |
| `ApplicationCommandTypeGuard` | Keeps handler bases and command types consistent. |
| `SlashCommandLocalizer`, `ContextCommandLocalizer` | Apply `ICommandLocalizationProvider` / `IContextCommandLocalizationProvider` to the built commands. |
| `CommandAutoRegisterModule` | `ICommandsUpdateWindowModule`: globals go to `OpenForGlobal(overwrite: false)`; commands with `GuildIds` go to each listed guild; `[OnDemand]` without guilds stays dormant. |

## 5. Public API
- Builder: `WithSlashCommands`, `WithContextMenuCommands`, `WithCommandLocalization`, `WithParamProvider<T>()`,
  `WithObjectConverter` (SPEC-SDK-01).
- Attributes and handler bases (§2).
- `ICommandRegistry` (read-only view of the registered commands).
- `ICommandContext.GetOption<T>` (`IWithOptionCollection`), `IAutoComplete`.
- Fluent definitions: `ICommandUpdateScopeBuilder.AddSlash`, `ICommandUpdateScopeBuilder.AddFromCatalog`
  (SPEC-API-008).

## 6. Discord surface
None directly. The built `ApplicationCommand` payloads are sent by the command update session
(SPEC-API-008), and responses go through the interaction actions (SPEC-API-009).

## 7. Flows
**Start-up**
1. `Build()` runs the scanners on the configured assemblies.
2. Each definition goes through `SlashCommandBuilder` validation, then the localizer, then the builder
   (`AddSlashFlat` / `AddSlashGroup` / context-menu and autocomplete adds).
3. `CommandRegistryBuilder.Build()` freezes the indexes. The registry and the dispatchers are registered in
   the SDK container.

**First READY**
1. `CommandAutoRegisterModule.OnCommandsUpdateWindowOpenedAsync` queues the auto-register entries.
2. The orchestrator commits every scope in the session (SPEC-SDK-01 §7).

**Slash invocation**
1. `INTERACTION_CREATE` (type 2, command type 1) produces an `ICommandContext`.
2. `SlashCommandDispatcher` calls `FindSlash(context.Name)`. With a subcommand, it calls
   `Group.FindCommand(group, subcommand)`; otherwise it uses `Flat`.
3. `CommandInfo.ExecuteAsync(context, services, default)`:
   1. resolve the handler from the event scope (scoped registration; `ActivatorUtilities` fallback);
   2. bind each parameter;
   3. invoke the method;
   4. adapt the result (await the task, or execute the `IRestAction`).
   With `[FireAndForget]` on the method, steps 3–4 run on `Task.Run` and errors are logged by
   `CommandInfo`; `SkipNextExecutions` is set synchronously first.

**Autocomplete**
`AutoCompleteName.FromContext`, then `FindAutoComplete`, then the `IAutoComplete` instance is created
with dependency injection and executed.

## 8. Concurrency and lifecycle
- The registry is immutable after `Build()` and safe for concurrent reads.
- Handler classes are scoped, so there is one instance per event scope. Dispatch concurrency follows SPEC-SDK-02 (per-shard
  ordering, `[FireAndForget]` opt-in).
- The command `CancellationToken` is currently `default`, so it never fires.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Duplicate command name, or a flat command and a group with the same name | `InvalidOperationException` at `Build()`. |
| Invalid name, description or option order | Validation exception at `Build()` (`SlashCommandValidationTests`). |
| Interaction for an unknown command or subcommand | Ignored silently: the dispatcher returns, so the interaction fails client-side after 3 s unless another handler responds. |
| Option conversion fails | The exception reaches the dispatcher's error log (SPEC-SDK-02). No user-facing reply. |
| Handler returns an `IRestAction` | Executed by `RestResult`. |
| `[OnDemand]` without guild ids | Not registered at start-up; registered at runtime through `IGuildCommandUpdateFactory`. |

## 10. Observability
Command handler spans and metrics come from the dispatcher (`discosdk.handler.invoke`, SPEC-SDK-05). There
are no command-specific tags such as the command name.

## 11. Tests
| Test class | Covers |
|---|---|
| `SlashCommandRegistryTests`, `SlashCommandGroupingTests`, `SlashCommandRoutingTests` | CF-F01, CF-F02, CF-F13 |
| `CommandRegistryTests`, `CommandRegistryBuilderTests` | CF-F13, CF-F14, CF-N01 |
| `ContextMenuCommandRegistryTests`, `ContextMenuContextTests`, `DispatchByCommandTypeTests` | CF-F04 |
| `SlashCommandValidationTests`, `SlashCommandAttributeContextsTests` | CF-F06, CF-F07, CF-F14, CF-N02 |
| `OnDemandRegistrationTests` | CF-F05 |
| `AutoCompleteSubcommandTests` | CF-F08 |
| `CommandContextExtractionTests`, `OptionValueConverterTests` | CF-F03, CF-F09, CF-F10, CF-F11 |
| `SlashCommandLocalizerTests`, `ContextCommandLocalizerTests`, `InMemoryCommandLocalizationProviderTests`, `InMemoryContextCommandLocalizationProviderTests`, `CommandLocalizationBuilderTests` | CF-F12 |
| `FluentSlashCommandBuilderTests`, `CommandUpdateScopeAddFromCatalogTests` | CF-F15 |

## 12. History
| Commit | Change |
|---|---|
| `649a310` | Scanner-based registration wired through the builder. |
| `34d688a` | Autocomplete classes with dependency injection. |
| `efb1745` | `Autocomplete*` → `AutoComplete*` rename. |

Next steps:
1. Add preconditions (`IPrecondition`, `[RequireUserPermission]`, `[Cooldown]`), CF-F16.
2. Pass a real cancellation token (the shutdown token, or the 15-minute interaction token lifetime).
3. Add a command-name tag to handler spans.
4. Log unknown commands at `Debug`.

## 13. Decisions and rejected alternatives
- **Freeze at build time**: reflection cost is paid once, and invalid definitions fail at start-up
  instead of at the first invocation.
- **Same definition for registration and routing**: the payload sent to Discord and the routing table
  cannot drift apart.
- **No prefix commands**: Discord's message-content policy makes application commands the supported path.
