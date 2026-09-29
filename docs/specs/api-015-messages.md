# SPEC-API-015 — Messages

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-015](../prd/api-015-messages.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/message](https://docs.discord.com/developers/resources/message) |
| **Last updated** | 2026-09-29 |

## 1. Summary
Messages are wrapped as `MessageWrapper` (`IMessage`). Channel-level operations live on `ITextBasedChannel`
(`TextBasedChannelWrapper`). Create and edit share `MessageBuilderAction<TSelf, TResult>`, which
accumulates content, embeds, components, attachments, allowed mentions, poll, stickers and flags, then
validates. The concrete actions are `SendMessageRestAction` (channel sends, replies, forwards, and
interaction replies with a follow-up fallback) and `EditMessageRestAction`. `MessageClient` implements
the routes; payloads with attachments use multipart (`payload_json` + `files[n]`).

## 2. Projects and dependencies
- `DiscoSdk/Models/Messages/**`: `IMessage`, `Embed*`, `EmbedBuilder`, `MessageReference`, `MessageFile`,
  mentions, and `MessageType`, `MessageFlags`, `ReactionType`.
- `DiscoSdk/Rest/Actions/Messages/*`: builder interfaces and `AllowedMentions`.
- `DiscoSdk.Hosting/Rest/Actions/Messages/*`, `Rest/Clients/MessageClient.cs`, `Wrappers/Messages/*`,
  `Rest/Actions/MessagePaginationAction.cs`, `GetReactionsAction`, `ReactionPaginationAction`.

## 3. Models and contracts
- The wire `Message` reads the documented fields, including `message_snapshots`. Only the deprecated `interaction` is read, not
  `interaction_metadata`.
- `MessageCreateRequest` covers `content`, `nonce`, `enforce_nonce`, `tts`, `embeds`, `allowed_mentions`,
  `message_reference`, `components`, `sticker_ids`, `attachments`, `flags` and `poll`.

## 4. Components
| Type | Responsibility |
|---|---|
| `MessageBuilderAction` | Accumulates the fields; `BuildFlags` (suppress embeds, V2); validation (content ≤ 2000, a non-empty body, V2 exclusivity). |
| `SendMessageRestAction` | `POST` channel message, or interaction reply / follow-up (SPEC-API-009). |
| `EditMessageRestAction` | `PATCH` with the attachment keep-list (`BuildAttachmentMetadataToEdit`). |
| `MessageClient` | Get, create, edit, delete, crosspost, reactions, bulk delete, deprecated pins, poll voters and end poll. |
| `MessagePaginationAction` | `around` / `before` / `after` / `limit` history. |

## 5. Public API
- `ITextBasedChannel`: `SendMessage`, `GetMessages`, `GetMessageAsync`, `EditMessageById`,
  `DeleteMessageAsync`, `BulkDeleteMessagesAsync`, `PurgeMessages*`, reactions by id, pins by id,
  `RetrievePinnedMessages`, `TriggerTypingAsync`.
- `IMessage`: `Edit`, `Delete`, `Reply`, `ForwardTo`, `ToBuilder`, `AddReaction`, `GetReactions`,
  `DeleteAllReactions*`, `Pin` / `Unpin`, `Crosspost`.

## 6. Discord surface
There are 20 message routes at the baseline. The SDK uses the 3 deprecated pin routes instead of the 3
current ones, and does not implement guild message search. It supports 35 of 37 message types
(`PURCHASE_NOTIFICATION` and `POLL_RESULT` are missing).

## 7. Flows
**Send with a file**
1. Builder → `MessageCreateRequest` plus `MessageFile[]`.
2. `MessageClient.CreateAsync` → `SendMultipartAsync` (JSON in `payload_json`, files as `files[n]`).
3. The response message is wrapped.

**Edit, keeping attachments**
`ToBuilder()` copies the original. `EditMessageRestAction` sends `attachments: [{ id }]` for the kept
files, plus the new uploads.

## 8. Concurrency and lifecycle
Builders are single-use. Wrappers are snapshots.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Empty message (no content, embeds, poll, attachment or V2 components) | `InvalidOperationException`. |
| Content over 2000 characters | `ArgumentException`. |
| Pin, reaction or crosspost on an ephemeral message | `EphemeralMessageException`. |
| Bulk delete with messages older than 14 days | Discord 400. `PurgeMessages*` should filter them first. |
| Empty `MentionBuilder` | No `allowed_mentions` is sent, so Discord pings everything mentioned in the content (see MS-F60). |

## 10. Observability
REST metrics only.

## 11. Tests
| Test class | Covers |
|---|---|
| `MessageClientTests` | MS-F01, MS-F03 – MS-F14, MS-F18 – MS-F20 |
| `MessageWrapperTests`, `TextBasedChannelWrapperTests` | MS-F04 – MS-F14, MS-F61 – MS-F63, MS-F70, MS-N02 |
| `GetReactionsActionTests`, `ReactionWrapperTests` | MS-F06 – MS-F11, MS-F69 |
| `EmbedBuilderTests` | MS-F59 |
| `MentionBuilderTests`, `MessageTextBuilderTests` | MS-F60 |
| `MessageForwardSerializationTests` | MS-F62 |
| `ClientMessageIntegrationTests`, `MessageExampleCommandTests` | MS-F04, MS-F58, MS-F63 – MS-F67 |
| `MessageVariantsDispatchTests` | MS-F21 – MS-F57 (message types) |

Implemented or Partial requirements without a covering test: MS-N01, MS-N03.

## 12. History
| Commit | Change |
|---|---|
| `9fe16f0` | Forward semantics (`message_reference.type` + `message_snapshots`). |
| `70f78e8` | `IMessage.ToBuilder` full-fidelity fork. |
| `22727cf` | Components V2 in messages. |

Next steps:
1. Switch pins to `/channels/{id}/messages/pins` with pagination.
2. Add guild message search.
3. Support "no mentions" and `replied_user`.
4. Honour `ReactionType` in the reaction listing.
5. Add a `nonce` / `enforce_nonce` setter.
6. Add voice messages.

## 13. Decisions and rejected alternatives
- **One builder base for create and edit**: validation and serialisation live in one place, and edits
  become "fork the message, change it, send it" (`ToBuilder`).
