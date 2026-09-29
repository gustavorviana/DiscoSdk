# SPEC-API-009 — Interactions (receiving and responding)

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-009](../prd/api-009-interactions.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [interactions/receiving-and-responding](https://docs.discord.com/developers/interactions/receiving-and-responding) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`INTERACTION_CREATE` is deserialised into `Interaction` (wire model) and wrapped as `InteractionWrapper`
(`IInteraction`). An `InteractionHandle` carries the id, token, application id and the `Responded` and
`SkipNextExecutions` flags for the whole dispatch chain. Response actions (`Reply`, `Defer`, `ReplyModal`,
`LaunchActivity`, autocomplete results) check the handle. Before the first acknowledgement they `POST` the
callback; afterwards, `Reply` becomes a follow-up (`POST /webhooks/{app}/{token}`), and `Edit` and `Delete`
target `@original`.

## 2. Projects and dependencies
- `DiscoSdk`: `IInteraction`, `IInteractionData`, `IInteractionResolved`, contexts
  (`IInteractionContext`, `ICommandContext`, `IAutocompleteContext`, `IModalContext`, …), `InteractionType`,
  `InteractionCallbackType`, `InteractionContextType`.
- `DiscoSdk.Hosting`: `InteractionClient`, `WebhookMessageClient`, `InteractionWrapper`,
  `InteractionContextWrapper`, `InteractionHandle`, `SendMessageRestAction`, `EditMessageRestAction`,
  `ReplyModalRestAction`, `LaunchActivityRestAction`.

## 3. Models and contracts
- Wire `Interaction`: `id`, `application_id`, `type`, `data`, `guild_id`, `channel_id`, `member`, `user`,
  `token`, `version`, `message`, `locale`, `guild_locale`. Missing: `app_permissions`, `entitlements`,
  `authorizing_integration_owners`, `context`, `attachment_size_limit`, `channel`.
- `InteractionCallbackRequest { type, data }`, where `data` is `InteractionCallbackData` (message fields),
  `ModalData` or autocomplete choices.

## 4. Components
| Type | Responsibility |
|---|---|
| `InteractionClient.SendCallbackAsync(handle, data, type)` | `POST /interactions/{id}/{token}/callback`; `with_response` is used for launch activity. |
| `InteractionClient.AcknowledgeAsync(handle, AcknowledgeType)` | Defer: type 5 with an optional ephemeral flag, or type 6 for a modal submit. |
| `InteractionClient.FollowUpAsync` | `POST /webhooks/{app}/{token}` (JSON or multipart). |
| `WebhookMessageClient` | `@original` and follow-up get/edit/delete. |
| `InteractionHandle` | Chain-scoped `Responded` and `SkipNextExecutions`. |

## 5. Public API
- `IInteractionContext`: `Interaction`, `Defer(ephemeral)`, `Reply(content)`, `ReplyModal()`, `LaunchActivity()`.
- `IInteraction`: `Defer`, `Reply`, `ReplyModal`, `Edit`, `Delete`.
- Handlers: `IApplicationCommandHandler`, `IUserCommandHandler`, `IMessageCommandHandler`,
  `IAutoCompleteHandler`, `IComponentInteractionHandler`, `IModalSubmitHandler`, `IInteractionCreateHandler`.

## 6. Discord surface
- `POST /interactions/{id}/{token}/callback`, and `GET`/`PATCH`/`DELETE`
  `/webhooks/{app}/{token}/messages/@original`.
- `POST /webhooks/{app}/{token}`, and `GET`/`PATCH`/`DELETE` `/webhooks/{app}/{token}/messages/{id}`.
- Callback types sent: 4, 5, 6 (modal submit only), 8, 9, 12. Not sent: 1, 7.

## 7. Flows
1. Dispatch creates the handle (`Responded=false`).
2. The handler calls `Reply()`:
   - if not responded → callback type 4, and set `Responded`;
   - otherwise → follow-up.
3. `Defer()` → type 5 (ephemeral flag optional), which sets `Responded`. A later `Edit()` patches `@original`.
4. The typed-handler chain stops once `Responded` is set (SPEC-SDK-02), so exactly one handler answers.

## 8. Concurrency and lifecycle
The token is valid for 15 minutes. The SDK does not track expiry, so an operation after 15 minutes gets
Discord's 401 (`Unknown Webhook`).

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Acknowledgement not sent within 3 s | Discord shows "This interaction failed". Handlers should `Defer` early (a `[FireAndForget]` handler must defer before detaching). |
| `Edit()` without a channel id | `InvalidOperationException`. |
| Component wants to update its own message | **Current:** no `UPDATE_MESSAGE`. The workaround is `Defer()` plus editing the message through the channel. **Proposed:** `Update()` / `DeferUpdate()` on component contexts. |
| Ephemeral message pin, reaction, crosspost or delete-all-reactions | `EphemeralMessageException`, before any request. |

## 10. Observability
Interaction dispatch spans and handler metrics (SPEC-SDK-05). The token is never logged.

## 11. Tests
| Test class | Covers |
|---|---|
| `InteractionClientTests` | IN-F11, IN-F15, IN-F25 – IN-F30, IN-F32 |
| `InteractionWrapperTests`, `InteractionDispatchTests` | IN-F01, IN-F03, IN-F05 – IN-F08 |
| `WebhookMessageClientTests`, `WebhookMessageWrapperTests` | IN-F12 – IN-F18 |
| `ClientModalIntegrationTests`, `ModalExampleCommandTests` | IN-F30 |
| `InteractionComponentConverterTests` | IN-F21, IN-F23 |
| `CommandContextExtractionTests`, `ContextMenuContextTests` | IN-F20, IN-F33 – IN-F35 |

## 12. History
| Commit | Change |
|---|---|
| `4898d06` | `LAUNCH_ACTIVITY` callback and the fix for the responded-on-ack rule. |
| `22727cf` | Components V2 and modal builders. |
| `506fd73` | Chain-break semantics for interaction handlers. |

Next steps:
1. Add `IComponentInteractionContext.Update()` / `DeferUpdate()` (types 7 and 6).
2. Deserialise the missing interaction fields.
3. Add public getters for `@original` and follow-ups.
4. Add an HTTP endpoint package that shares the verifier with SPEC-API-005.

## 13. Decisions and rejected alternatives
- **One handle per dispatch chain**: exactly one handler answers, and a second `Reply` turns into a follow-up
  instead of a Discord 400.
