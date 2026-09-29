# PRD-API-010 — Components

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-010](../specs/api-010-components.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [components/overview](https://docs.discord.com/developers/components/overview), [components/reference](https://docs.discord.com/developers/components/reference), [components/using-message-components](https://docs.discord.com/developers/components/using-message-components), [components/using-modal-components](https://docs.discord.com/developers/components/using-modal-components) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Discord messages and modals are built from a component tree: action rows, buttons, selects, text inputs,
and the V2 layout components (section, container, media gallery, …), plus the newer modal inputs (label,
file upload, radio group, checkbox). The component JSON is polymorphic, nesting rules differ between
messages and modals, and V2 messages need the `IS_COMPONENTS_V2` flag. Hand-writing it is fragile.

## 2. Goals
- A typed model and a fluent builder for every component type.
- Polymorphic (de)serialisation that round-trips unknown and future fields safely.
- Submitted values from modal and message components are read through typed contexts.

## 3. Out of scope
- Sending messages: [PRD-API-015](api-015-messages.md). Responding to component interactions:
  [PRD-API-009](api-009-interactions.md).

## 4. Usage scenarios
- As a bot author, I send a container with a section, a thumbnail and a button row. The SDK sets
  `IS_COMPONENTS_V2`.
- A modal with a label-wrapped radio group and a file upload returns typed values in `IModalContext`.

## 5. Desired developer experience

```csharp
await channel.SendMessage()
    .AddComponent(new ContainerBuilder()
        .WithAccentColor(0x5865F2)
        .AddTextDisplay("## Deploy finished")
        .AddSeparator()
        .Build())
    .AddActionRow(ButtonBuilder.Link(logsUrl, "Logs"))
    .ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| CP-F01 | Common component fields (`type`, optional numeric `id`) | — | Must | Implemented | `MessageComponent.Id` |
| CP-F02 | `IS_COMPONENTS_V2` flag set automatically for V2 layouts | — | Must | Implemented | `MessageBuilderAction` (`HasComponentsV2`) → `MessageFlags.IsComponentV2` |
| CP-F03 | Polymorphic (de)serialisation for V1 + V2, including nested trees | — | Must | Implemented | `MessageComponentPolymorphicConverter`, `ComponentTypeMapping` |
| CP-F04 | Button styles 1–6, including link and premium (`sku_id`) | — | Must | Implemented | `ButtonStyle` (`Link`, `Premium`), `ButtonComponent.SkuId` |
| CP-F05 | Select default values (user/role/channel) and select options | — | Must | Implemented | `SelectDefaultValue`, `SelectOption` |
| CP-F06 | Unfurled media item (URL or `attachment://` reference) | — | Must | Implemented | `UnfurledMediaItem` |
| CP-F07 | Unfurled media item flags (`IS_ANIMATED`) and resolved metadata (width, height, content type, loading state) | — | Could | Partial | `UnfurledMediaItem` carries url, proxy url, width, height and content type, but has no flags enum |
| CP-F08 | Modal submit values per component (text input, selects, radio, checkbox, checkbox group, file upload) | — | Must | Implemented | `IModalContext`, `ModalSubmitRow`, `ModalSubmitField` |
| CP-F09 | Message component interaction data (`custom_id`, `component_type`, `values`, resolved) | — | Must | Implemented | `IInteractionData`, `IComponentInteractionHandler` |
| CP-F10 | Content and size limits (5 rows, 25 selects options, 40 total V2 components, text lengths) validated client-side | — | Should | Partial | Builders validate lengths and counts per component; there is no whole-message V2 component budget check |
| CP-F11 | Component `Action Row` (1) | `component:1` | Must | Implemented | `ComponentType` → `MessageActionRowComponent` |
| CP-F12 | Component `Button` (2) | `component:2` | Must | Implemented | `ComponentType` → `ButtonComponent`, `ButtonBuilder` |
| CP-F13 | Component `String Select` (3) | `component:3` | Must | Implemented | `ComponentType` → `StringSelectComponent`, `StringSelectBuilder` |
| CP-F14 | Component `Text Input` (4) | `component:4` | Must | Implemented | `ComponentType` → `TextInputComponent`, `TextInputBuilder` |
| CP-F15 | Component `User Select` (5) | `component:5` | Must | Implemented | `ComponentType` → `UserSelectComponent`, `UserSelectBuilder` |
| CP-F16 | Component `Role Select` (6) | `component:6` | Must | Implemented | `ComponentType` → `RoleSelectComponent`, `RoleSelectBuilder` |
| CP-F17 | Component `Mentionable Select` (7) | `component:7` | Must | Implemented | `ComponentType` → `MentionableSelectComponent`, `MentionableSelectBuilder` |
| CP-F18 | Component `Channel Select` (8) | `component:8` | Must | Implemented | `ComponentType` → `ChannelSelectComponent`, `ChannelSelectBuilder` |
| CP-F19 | Component `Section` (9) | `component:9` | Must | Implemented | `ComponentType` → `SectionComponent`, `SectionBuilder` |
| CP-F20 | Component `Text Display` (10) | `component:10` | Must | Implemented | `ComponentType` → `TextDisplayComponent` |
| CP-F21 | Component `Thumbnail` (11) | `component:11` | Must | Implemented | `ComponentType` → `ThumbnailComponent` |
| CP-F22 | Component `Media Gallery` (12) | `component:12` | Must | Implemented | `ComponentType` → `MediaGalleryComponent`, `MediaGalleryBuilder` |
| CP-F23 | Component `File` (13) | `component:13` | Must | Implemented | `ComponentType` → `FileComponent` |
| CP-F24 | Component `Separator` (14) | `component:14` | Must | Implemented | `ComponentType` → `SeparatorComponent` |
| CP-F25 | Component `Container` (17) | `component:17` | Must | Implemented | `ComponentType` → `ContainerComponent`, `ContainerBuilder` |
| CP-F26 | Component `Label` (18) | `component:18` | Must | Implemented | `ComponentType` → `LabelComponent` |
| CP-F27 | Component `File Upload` (19) | `component:19` | Must | Implemented | `ComponentType` → `FileUploadComponent`, `FileUploadBuilder` |
| CP-F28 | Component `Radio Group` (21) | `component:21` | Must | Implemented | `ComponentType` → `RadioGroupComponent`, `RadioGroupBuilder` |
| CP-F29 | Component `Checkbox Group` (22) | `component:22` | Must | Implemented | `ComponentType` → `CheckboxGroupComponent`, `CheckboxGroupBuilder` |
| CP-F30 | Component `Checkbox` (23) | `component:23` | Must | Implemented | `ComponentType` → `CheckboxComponent` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| CP-N01 | Deserialising an unknown component type does not throw. It falls back to the generic `MessageComponent` (`ComponentTypeMapping`), so messages using newer components still parse. |
| CP-N02 | Every builder produces a plain model (`Build()`), so the same model can be attached to several messages. |

## 8. Configuration
None.

## 9. Compatibility
A new component type is additive: new model, new builder, and a new mapping entry in `ComponentTypeMapping`.

## 10. Acceptance criteria
- [x] Every component type serialises to the documented JSON and round-trips (`AllComponentsSerializationTests`, `ComponentsV2SerializationTests`, `ModalComponentsExtendedTests`).
- [x] `TextInputBuilder` validation (`TextInputBuilderTests`).
- [x] Modal submissions expose typed values (`ClientModalIntegrationTests`, `ModalExampleCommandTests`).
- [ ] A V2 message over 40 components is rejected before sending (CP-F10).

## 11. Open questions
- None.
