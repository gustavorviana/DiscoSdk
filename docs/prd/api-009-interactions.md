# PRD-API-009 — Interactions (receiving and responding)

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-009](../specs/api-009-interactions.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [interactions/overview](https://docs.discord.com/developers/interactions/overview), [interactions/receiving-and-responding](https://docs.discord.com/developers/interactions/receiving-and-responding) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Every command, button, select menu, autocomplete and modal reaches the bot as an interaction. The bot must
acknowledge it within 3 seconds with one of nine callback types, and then has 15 minutes of follow-ups on
the interaction token. A wrong callback type, or a double acknowledgement, leaves users with "This
interaction failed".

## 2. Goals
- Receive interactions over the gateway, and optionally over the HTTP interactions endpoint.
- Offer typed respond operations: reply, defer, update, modal, autocomplete, launch activity, follow-ups.
  The SDK prevents responding twice.
- Expose the whole interaction object, including `app_permissions`, `entitlements`, `context` and the
  authorizing integration owners.

## 3. Out of scope
- The command framework (routing to handler methods, parameter binding): [PRD-SDK-03](sdk-03-command-framework.md).
- Component and modal payload structures: [PRD-API-010](api-010-components.md).
- The message-building API used by replies: [PRD-API-015](api-015-messages.md).

## 4. Usage scenarios
- As a bot author, a slow command calls `Defer()`, and after the database call finishes it edits the
  original response.
- A button handler updates the message the button lives on, without creating a new message.
- An ephemeral error reply is visible only to the invoking user.

## 5. Desired developer experience

```csharp
public sealed class SlowQuery : IApplicationCommandHandler
{
    public async Task HandleAsync(ICommandContext context)
    {
        await context.Defer(ephemeral: true).ExecuteAsync();
        var result = await RunQueryAsync();
        await context.Interaction.Edit().SetContent(result).ExecuteAsync();
    }
}
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| IN-F01 | Receive interactions over the gateway (`INTERACTION_CREATE`) | — | Must | Implemented | `InteractionContextWrapper`, `DiscordEventDispatcher` (PRD-API-004) |
| IN-F02 | HTTP interactions endpoint (Interactions Endpoint URL, Ed25519 verification, PING/PONG) | — | Should | Missing | No HTTP receiver; `IApplication.InteractionsEndpointUrl` is readable only. Shares the verifier proposed in SPEC-API-005. |
| IN-F03 | Interaction object: id, application id, type, data, guild, channel, member, user, token, version, message, locales | — | Must | Implemented | `IInteraction` |
| IN-F04 | Interaction object: `app_permissions`, `entitlements`, `authorizing_integration_owners`, `context`, `attachment_size_limit` | — | Must | Missing | Not deserialised (`Interaction` model) |
| IN-F05 | Resolved data (users, members, roles, channels, messages, attachments) | — | Must | Implemented | `IInteractionData`, `IInteractionResolved` |
| IN-F06 | Double-response guard and 3-second acknowledgement semantics | — | Must | Implemented | `InteractionHandle.Responded` (commit `4898d06`) |
| IN-F07 | Ephemeral responses | — | Must | Implemented | `IInteractionContext.Defer(ephemeral)`, `ISendMessageRestAction` ephemeral flag |
| IN-F08 | Follow-up messages within the 15-minute token window | — | Must | Implemented | `IInteractionContext.Reply` after acknowledgement → `InteractionClient.FollowUpAsync` |
| IN-F09 | Callback response with `with_response=true` (returns the interaction callback resource) | — | Should | Partial | Used for `LAUNCH_ACTIVITY` only (`ILaunchActivityRestAction`); other callbacks discard the response |
| IN-F10 | Message interaction metadata on messages created by interactions | — | Should | Partial | The wire `Message` reads only the deprecated `interaction` field (`MessageInteraction`). `interaction_metadata` is not parsed, and neither is exposed on `IMessage`. |
| IN-F11 | Create Interaction Response | `POST /interactions/{}/{}/callback` | Must | Implemented | `IInteractionContext.Reply()`, `IInteractionContext.Defer()`, `IInteractionContext.ReplyModal()`, `IInteractionContext.LaunchActivity()` → `InteractionClient.SendCallbackAsync` |
| IN-F12 | Get Original Interaction Response | `GET /webhooks/{}/{}/messages/@original` | Must | Implemented | `WebhookMessageClient.GetOriginalResponseAsync` (used internally when editing; no public getter) |
| IN-F13 | Edit Original Interaction Response | `PATCH /webhooks/{}/{}/messages/@original` | Must | Implemented | `IInteraction.Edit()` → `WebhookMessageClient.EditOriginalResponseAsync` |
| IN-F14 | Delete Original Interaction Response | `DELETE /webhooks/{}/{}/messages/@original` | Must | Implemented | `IInteraction.Delete()` → `WebhookMessageClient.DeleteOriginalResponseAsync` |
| IN-F15 | Create Followup Message / Execute Webhook | `POST /webhooks/{}/{}` | Must | Implemented | `IInteractionContext.Reply()` after acknowledgement → `InteractionClient.FollowUpAsync`; webhooks → `WebhookMessageClient.ExecuteAsync` (PRD-API-026) |
| IN-F16 | Get Followup Message / Get Webhook Message | `GET /webhooks/{}/{}/messages/{}` | Must | Implemented | `WebhookMessageClient.GetAsync` (webhook-message API; no follow-up getter on interaction contexts) |
| IN-F17 | Edit Followup Message / Edit Webhook Message | `PATCH /webhooks/{}/{}/messages/{}` | Must | Implemented | `WebhookMessageClient.EditAsync` |
| IN-F18 | Delete Followup Message / Delete Webhook Message | `DELETE /webhooks/{}/{}/messages/{}` | Must | Implemented | `WebhookMessageClient.DeleteAsync` |
| IN-F19 | Interaction type `PING` (1) | `itype:1` | Must | Partial | `InteractionType.Ping` exists; only relevant to the HTTP endpoint (IN-F02) |
| IN-F20 | Interaction type `APPLICATION_COMMAND` (2) | `itype:2` | Must | Implemented | `InteractionType.ApplicationCommand` |
| IN-F21 | Interaction type `MESSAGE_COMPONENT` (3) | `itype:3` | Must | Implemented | `InteractionType.MessageComponent` |
| IN-F22 | Interaction type `APPLICATION_COMMAND_AUTOCOMPLETE` (4) | `itype:4` | Must | Implemented | `InteractionType.ApplicationCommandAutoComplete` |
| IN-F23 | Interaction type `MODAL_SUBMIT` (5) | `itype:5` | Must | Implemented | `InteractionType.ModalSubmit` |
| IN-F24 | Callback type `PONG` (1) | `callback:1` | Should | Partial | `InteractionCallbackType.Pong` exists; only needed by the missing HTTP endpoint (IN-F02) |
| IN-F25 | Callback type `CHANNEL_MESSAGE_WITH_SOURCE` (4) | `callback:4` | Must | Implemented | `InteractionCallbackType.ChannelMessageWithSource` |
| IN-F26 | Callback type `DEFERRED_CHANNEL_MESSAGE_WITH_SOURCE` (5) | `callback:5` | Must | Implemented | `InteractionCallbackType.DeferredChannelMessageWithSource` |
| IN-F27 | Callback type `DEFERRED_UPDATE_MESSAGE` (6) | `callback:6` | Must | Partial | `InteractionCallbackType.DeferredUpdateMessage` is sent only for modal-submit acknowledgement (`InteractionClient.AcknowledgeAsync`); components cannot defer-update |
| IN-F28 | Callback type `UPDATE_MESSAGE` (7) | `callback:7` | Must | Partial | `InteractionCallbackType.UpdateMessage` is declared but never sent; components cannot update their message in the callback |
| IN-F29 | Callback type `APPLICATION_COMMAND_AUTOCOMPLETE_RESULT` (8) | `callback:8` | Must | Implemented | `InteractionCallbackType.ApplicationCommandAutoCompleteResult` |
| IN-F30 | Callback type `MODAL` (9) | `callback:9` | Must | Implemented | `InteractionCallbackType.Modal` |
| IN-F31 | Callback type `PREMIUM_REQUIRED` (10) | `callback:10` | Could | Deprecated | Replaced by premium buttons (component style 6) |
| IN-F32 | Callback type `LAUNCH_ACTIVITY` (12) | `callback:12` | Must | Implemented | `InteractionCallbackType.LaunchActivity` |
| IN-F33 | Interaction context `GUILD` (0) | `context:0` | Must | Implemented | `InteractionContextType.Guild` |
| IN-F34 | Interaction context `BOT_DM` (1) | `context:1` | Must | Implemented | `InteractionContextType.BotDm` |
| IN-F35 | Interaction context `PRIVATE_CHANNEL` (2) | `context:2` | Must | Implemented | `InteractionContextType.PrivateChannel` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| IN-N01 | Callback routes bypass the global REST rate limit (Discord exempts them). See PRD-API-002, RL-F06. |
| IN-N02 | The interaction token never appears in logs; it is a webhook credential. |

## 8. Configuration
None.

## 9. Compatibility
Adding the missing interaction fields is additive. An HTTP receiver would ship as a separate package
(PRD-API-005).

## 10. Acceptance criteria
- [x] Reply, defer, modal, autocomplete and launch-activity callbacks send the correct `type` (`InteractionClientTests`, `InteractionWrapperTests`).
- [x] Responding twice throws instead of sending a second callback (`InteractionWrapperTests`).
- [x] Follow-ups and original-response edit/delete use the webhook routes (`WebhookMessageClientTests`).
- [ ] Components can answer with `UPDATE_MESSAGE` / `DEFERRED_UPDATE_MESSAGE` (IN-F27, IN-F28).
- [ ] `app_permissions`, `entitlements` and `context` are exposed (IN-F04).

## 11. Open questions
- Should `IComponentInteractionContext` get `Update()` and `DeferUpdate()`? Provisional: yes. This is the most
  visible gap for component-heavy bots.
