# SPEC-API-010 — Components

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-010](../prd/api-010-components.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [components/reference](https://docs.discord.com/developers/components/reference) |
| **Last updated** | 2026-09-29 |

## 1. Summary
Each component type has a concrete model in `DiscoSdk/Models/Messages/Components`, implementing
`IMessageComponent` (top level in messages), `IInteractionComponent` (inside rows, sections and containers)
or `IModalComponent`. Fluent builders produce these models. `ComponentTypeMapping.Deserialize` maps `type`
to the concrete class, falling back to `MessageComponent`, and it is used by
`MessageComponentPolymorphicConverter`, `InteractionComponentConverter` and the modal converters, so
sending and receiving are symmetric. `MessageBuilderAction` detects V2 components and sets
`MessageFlags.IsComponentV2`.

## 2. Projects and dependencies
`DiscoSdk` only (models, builders, converters), plus `DiscoSdk.Hosting/Rest/Actions/Messages` for the flag
and multipart handling of `attachment://` media.

## 3. Models and contracts
- Layout: `MessageActionRowComponent` (message) / `ActionRowComponent` (modal), `SectionComponent`,
  `ContainerComponent`, `SeparatorComponent`, `LabelComponent`.
- Content: `TextDisplayComponent`, `ThumbnailComponent`, `MediaGalleryComponent` (+ `MediaGalleryItem`),
  `FileComponent`, `UnfurledMediaItem`.
- Interactive: `ButtonComponent`, the `*SelectComponent` family (+ `SelectOption`, `SelectDefaultValue`),
  `TextInputComponent`, `FileUploadComponent`, `RadioGroupComponent` (+ option),
  `CheckboxGroupComponent` (+ option), `CheckboxComponent`.
- Modal payloads: `ModalData` (sent), and `ModalSubmitRow` / `ModalSubmitField` (received).

## 4. Components
| Type | Responsibility |
|---|---|
| `ComponentTypeMapping` | `ComponentType` → concrete type; unknown → `MessageComponent`. |
| `MessageComponentPolymorphicConverter`, `InteractionComponentConverter` | JSON polymorphism for message trees. |
| `ButtonBuilder` (`Link`, `Premium` factories), select builders, `TextInputBuilder`, `SectionBuilder`, `ContainerBuilder`, `MediaGalleryBuilder`, `FileUploadBuilder`, `RadioGroupBuilder`, `CheckboxGroupBuilder` | Fluent construction and per-component validation. |
| `MessageBuilderAction.HasComponentsV2` | Sets the V2 flag when any top-level component is not an action row, and blocks V1 fields on V2 messages. |

## 5. Public API
`IMessageBuilderAction.AddActionRow` / `AddComponent`, `IReplyModalRestAction.AddActionRow` /
`SetComponents`, the builders listed above, and `IModalContext` for submitted values.

## 6. Discord surface
Component types 1–14, 17–19 and 21–23 (no gaps at the baseline), and button styles 1–6.

## 7. Flows
**Send V2**
1. The builder produces a `ContainerComponent`, and `AddComponent` stores it.
2. `MessageBuilderAction` sees a top-level component that is not an action row and ORs `IsComponentV2` into `flags`. It rejects V2 mixed with content, embeds, a poll or stickers, throwing `InvalidOperationException` before sending.

**Receive modal**
1. `ModalData.Components` → `ModalSubmitRow` → `ModalSubmitField` (value, values or file ids).
2. `IModalContext` exposes them by `custom_id`.

## 8. Concurrency and lifecycle
Models are plain DTOs, and builders are not thread-safe. Build a model per message when you share builders.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Component JSON without `type` | `JsonException` ("Component missing 'type' field."). |
| Unknown `type` | Deserialised as `MessageComponent`. |
| V2 message over the 40-component limit | **Current:** Discord 400. **Proposed:** builder validation. |

## 10. Observability
None.

## 11. Tests
| Test class | Covers |
|---|---|
| `AllComponentsSerializationTests`, `ComponentsV2SerializationTests` | CP-F01 – CP-F06, CP-F11 – CP-F30 |
| `ModalComponentsExtendedTests`, `TextInputBuilderTests` | CP-F08, CP-F10, CP-F14, CP-F27 – CP-F30 |
| `InteractionComponentConverterTests` | CP-F03, CP-F09, CP-N01 |
| `ClientModalIntegrationTests`, `ModalExampleCommandTests`, `MessageExampleCommandTests` | CP-F08, CP-F09 |

## 12. History
| Commit | Change |
|---|---|
| `22727cf` | Components V2, builders, symmetric polymorphism. |
| `4b5467a` | Inline `Action<TBuilder>` overloads. |

Next steps:
1. Add a V2 component budget check.
2. Add `UnfurledMediaItem` flags and loading state.

## 13. Decisions and rejected alternatives
- **Concrete class per type** rather than a single `MessageComponent` god-class: this gives compile-time
  safety for builders and readers. The god-class is kept only as the unknown-type fallback.
