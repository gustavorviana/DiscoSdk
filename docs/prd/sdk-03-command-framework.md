# PRD-SDK-03 — Command framework

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-SDK-03](../specs/sdk-03-command-framework.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — (SDK framework feature; Discord command surface in PRD-API-008) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Writing commands against raw interactions means parsing option trees, routing subcommand paths, converting
option values (users, channels, enums), wiring autocomplete, translating names and registering everything
with Discord. Bot authors want to declare a method with attributes and have the SDK handle the rest.

## 2. Goals
- Attribute-based slash and context-menu commands on handler classes. The same definitions feed Discord
  registration (PRD-API-008) and runtime routing.
- Parameter binding for options, context, services and cancellation, with custom parameter providers.
- Autocomplete handlers with dependency injection, and localization providers for names, descriptions and
  choices.
- A frozen, validated registry built once at start-up.

## 3. Out of scope
- Registration REST and diffing: [PRD-API-008](api-008-application-commands.md).
- Interaction responses: [PRD-API-009](api-009-interactions.md). Handler execution: [PRD-SDK-02](sdk-02-event-dispatch.md).

## 4. Usage scenarios
- As a bot author, I write `[SlashCommand("status")]` with an enum option, and the SDK offers the enum values
  as choices and converts the selected one back.
- `/config set` and `/config get` live as subcommands on one class.
- An autocomplete class suggests matching names and receives my services through its constructor.
- Command names are translated to `pt-BR` from an in-memory provider.

## 5. Desired developer experience

```csharp
public class StatusCommand : SlashCommandHandler
{
    [SlashCommand("status", "Update bot status.")]
    [SlashOption(SlashCommandOptionType.String, "status", "New bot status.", required: true)]
    [EnumChoices<OnlineStatus>(OptionName = "status", ExceptValues = [OnlineStatus.Offline])]
    protected async Task OnExecuteAsync(ICommandContext context)
    {
        var status = context.GetOption<OnlineStatus>("status");
        await context.Client.UpdatePresence().SetStatus(status!.Value).ExecuteAsync();
        await context.Reply("Ok").SetEphemeral().ExecuteAsync();
    }
}
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| CF-F01 | Attribute-based slash commands on handler classes | — | Must | Implemented | `SlashCommandHandler`, `SlashCommandAttribute`, `SlashOptionAttribute` → `SlashCommandScanner` |
| CF-F02 | Subcommands and subcommand groups by attribute | — | Must | Implemented | `SubCommandAttribute`, `SubCommandGroupAttribute` |
| CF-F03 | Choices: static choices and enum-derived choices with exclusions | — | Must | Implemented | `ChoiceAttribute`, `EnumChoicesAttribute` |
| CF-F04 | Context-menu (user and message) commands | — | Must | Implemented | `ContextMenuCommandAttribute`, `UserContextMenuHandler`, `MessageContextMenuHandler` → `ContextMenuCommandScanner` |
| CF-F05 | Guild-scoped and on-demand registration | — | Should | Implemented | `SlashCommandAttribute.GuildIds`, `OnDemandAttribute` |
| CF-F06 | Contexts and integration types on attributes | — | Must | Implemented | `SlashCommandAttribute.Contexts`, `SlashCommandAttribute.IntegrationTypes` |
| CF-F07 | Option constraints on attributes (min/max value, channel types) | — | Should | Implemented | `SlashOptionAttribute.MinValue`, `SlashOptionAttribute.MaxValue`, `SlashOptionAttribute.ChannelTypes` |
| CF-F08 | Autocomplete classes (dependency-injection constructed) bound by attribute or option | — | Must | Implemented | `IAutoComplete`, `AutoCompleteHandlerAttribute`, `SlashOptionAttribute.AutoCompleteType` (commit `34d688a`) |
| CF-F09 | Parameter binding: context, options, services (`[FromServices]`), `CancellationToken`, custom providers | — | Must | Implemented | `FromServicesAttribute`, `IParamProvider`, `UserParamProvider`, `MemberParamProvider`, `ChannelParamProvider`, `GuildParamProvider` |
| CF-F10 | Typed option access and conversion (enums, snowflakes, entities) | — | Must | Implemented | `IWithOptionCollection.GetOption`, `IObjectConverter` (`DiscordClientBuilder.WithObjectConverter`) |
| CF-F11 | Return handling: `void`, `Task`, or `IRestAction` (executed automatically) | — | Should | Implemented | `VoidResult`, `TaskResult`, `RestResult` |
| CF-F12 | Localization providers for commands and context menus (fluent builders, in-memory providers) | — | Should | Implemented | `ICommandLocalizationProvider`, `IContextCommandLocalizationProvider`, `InMemoryCommandLocalizationProvider`, `CommandLocalizationBuilder` |
| CF-F13 | Frozen registry with routing by name, subcommand path and command type | — | Must | Implemented | `CommandRegistry` (`FrozenDictionary`), `SlashCommandDispatcher`, `ContextMenuCommandDispatcher` |
| CF-F14 | Definition validation at start-up (names, option order, duplicates) | — | Must | Implemented | `SlashCommandBuilder` validation, `CommandRegistryBuilder` |
| CF-F15 | Fluent (non-attribute) definitions merged with discovered ones | — | Should | Implemented | `ICommandUpdateScopeBuilder.AddSlash`, `ICommandUpdateScopeBuilder.AddFromCatalog` |
| CF-F16 | Preconditions: permission checks, cooldowns, owner-only | — | Should | Missing | No precondition attributes; checks are written by hand in each handler |
| CF-F17 | Text/prefix commands | — | Could | Excluded | Discord's MESSAGE_CONTENT policy steers bots to application commands; the SDK does not plan prefix commands |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| CF-N01 | Reflection runs once at `Build()`. Routing is dictionary lookups on frozen collections. |
| CF-N02 | Invalid definitions fail at start-up with a descriptive exception, never at the first invocation. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Slash assemblies to scan | none | assemblies | `WithSlashCommands(params Assembly[])` |
| Context-menu assemblies to scan | none | assemblies | `WithContextMenuCommands(params Assembly[])` |
| Localization provider | none | provider type or instance | `WithCommandLocalization` |
| Parameter providers | built-in user/member/channel/guild | `IParamProvider` | `WithParamProvider<T>()` |
| Object converter | `ObjectConverter` (invariant culture) | `IObjectConverter` | `WithObjectConverter` |

## 9. Compatibility
Precondition attributes would be additive. The recent rename `Autocomplete*` → `AutoComplete*`
(commit `efb1745`) was breaking.

## 10. Acceptance criteria
- [x] Scanning, grouping and routing (`SlashCommandRegistryTests`, `SlashCommandGroupingTests`, `SlashCommandRoutingTests`, `CommandRegistryTests`, `CommandRegistryBuilderTests`, `ContextMenuCommandRegistryTests`).
- [x] Validation (`SlashCommandValidationTests`, `SlashCommandAttributeContextsTests`).
- [x] Autocomplete with subcommands and dependency injection (`AutoCompleteSubcommandTests`).
- [x] Parameter extraction (`CommandContextExtractionTests`, `ContextMenuContextTests`, `OptionValueConverterTests`).
- [x] Localization (`SlashCommandLocalizerTests`, `ContextCommandLocalizerTests`, `InMemoryCommandLocalizationProviderTests`, `InMemoryContextCommandLocalizationProviderTests`, `CommandLocalizationBuilderTests`).
- [ ] Preconditions (CF-F16).

## 11. Open questions
- Precondition model: attributes (`[RequireUserPermission]`, `[Cooldown]`) evaluated before the handler,
  with a typed failure result replied ephemerally? Provisional: yes, as `IPrecondition` with dependency-injection support.
