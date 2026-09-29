# PRD-API-015 — Messages

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-015](../specs/api-015-messages.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/message](https://docs.discord.com/developers/resources/message) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Messages are the most-used surface: sending with content, embeds, files, components, stickers, polls,
replies and forwards; editing while keeping or removing attachments; reactions; pins; history; bulk
deletion. Each part has limits (2000 characters, 10 embeds, 6000 embed characters, 25 fields, 100 bulk
deletes, messages younger than 14 days) and interacts with allowed mentions.

## 2. Goals
- One fluent builder for create and edit, with local validation of Discord limits.
- Full-fidelity editing (`IMessage.ToBuilder`) and forwarding (`ForwardTo`).
- Every message route reachable through `ITextBasedChannel` or `IMessage`, using Discord's current routes
  rather than deprecated ones.

## 3. Out of scope
- Components: [PRD-API-010](api-010-components.md). Polls: [PRD-API-016](api-016-polls.md). Interaction
  responses: [PRD-API-009](api-009-interactions.md). Webhook messages: [PRD-API-026](api-026-webhooks.md).

## 4. Usage scenarios
- As a bot author, I reply to a message without pinging its author (needs `replied_user: false`, see MS-F60).
- I edit my previous message, replace one attachment and keep the others.
- I purge the last 50 messages from a spammer in one request.
- I pin the release announcement.

## 5. Desired developer experience

```csharp
await message.Reply("Done!")
    .AttachFile(new MessageFile("report.pdf", "Weekly report", pdfBytes))
    .ExecuteAsync();

await channel.BulkDeleteMessagesAsync(spamIds).ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| MS-F01 | Get Channel Messages | `GET /channels/{}/messages` | Must | Implemented | `ITextBasedChannel.GetMessages()` → `MessagePaginationAction` → `ChannelClient.GetMessagesAsync` → `MessageClient.GetMessagesAsync` |
| MS-F02 | Search Guild Messages | `GET /guilds/{}/messages/search` | Should | Missing | — |
| MS-F03 | Get Channel Message | `GET /channels/{}/messages/{}` | Must | Implemented | `ITextBasedChannel.GetMessageAsync()` → `MessageClient.GetAsync` |
| MS-F04 | Create Message | `POST /channels/{}/messages` | Must | Implemented | `ITextBasedChannel.SendMessage()`, `IMessage.Reply()`, `IMessage.ForwardTo()` → `SendMessageRestAction` → `MessageClient.CreateAsync` |
| MS-F05 | Crosspost Message | `POST /channels/{}/messages/{}/crosspost` | Must | Implemented | `IGuildNewsChannel.CrosspostMessage()`, `IMessage.Crosspost()` → `MessageClient.CrosspostAsync` |
| MS-F06 | Create Reaction | `PUT /channels/{}/messages/{}/reactions/{}/@me` | Must | Implemented | `IMessage.AddReaction()`, `ITextBasedChannel.AddReactionByIdAsync()` → `MessageClient.AddReactionAsync` |
| MS-F07 | Delete Own Reaction | `DELETE /channels/{}/messages/{}/reactions/{}/@me` | Must | Implemented | `IReaction.Delete()`, `ITextBasedChannel.RemoveReactionByIdAsync()` → `MessageClient.RemoveReactionAsync` |
| MS-F08 | Delete User Reaction | `DELETE /channels/{}/messages/{}/reactions/{}/{}` | Must | Implemented | `IReaction.Delete()` → `MessageClient.RemoveUserReactionAsync` |
| MS-F09 | Get Reactions | `GET /channels/{}/messages/{}/reactions/{}` | Must | Implemented | `IMessage.GetReactions()`, `ITextBasedChannel.RetrieveReactionUsersById()` → `MessageClient.GetReactionsAsync` (see MS-F69) |
| MS-F10 | Delete All Reactions | `DELETE /channels/{}/messages/{}/reactions` | Must | Implemented | `IMessage.DeleteAllReactions()` → `MessageClient.DeleteAllReactionsAsync` |
| MS-F11 | Delete All Reactions for Emoji | `DELETE /channels/{}/messages/{}/reactions/{}` | Must | Implemented | `IMessage.DeleteAllReactionsForEmoji()` → `MessageClient.DeleteAllReactionsForEmojiAsync` |
| MS-F12 | Edit Message | `PATCH /channels/{}/messages/{}` | Must | Implemented | `IInteraction.Edit()`, `IMessage.Edit()` → `MessageClient.EditAsync` |
| MS-F13 | Delete Message | `DELETE /channels/{}/messages/{}` | Must | Implemented | `IMessage.Delete()` → `MessageClient.DeleteAsync` |
| MS-F14 | Bulk Delete Messages | `POST /channels/{}/messages/bulk-delete` | Must | Implemented | `ITextBasedChannel.BulkDeleteMessagesAsync()`, `ITextBasedChannel.PurgeMessagesAsync()` → `MessageClient.BulkDeleteMessagesAsync` |
| MS-F15 | Get Channel Pins | `GET /channels/{}/messages/pins` | Must | Missing | The SDK still uses the deprecated `GET /channels/{id}/pins` (MS-F18) |
| MS-F16 | Pin Message | `PUT /channels/{}/messages/pins/{}` | Must | Missing | The SDK still uses the deprecated `PUT /channels/{id}/pins/{id}` (MS-F19) |
| MS-F17 | Unpin Message | `DELETE /channels/{}/messages/pins/{}` | Must | Missing | The SDK still uses the deprecated `DELETE /channels/{id}/pins/{id}` (MS-F20) |
| MS-F18 | Get Pinned Messages (deprecated) | `GET /channels/{}/pins` | Should | Deprecated | Still used: `ITextBasedChannel.RetrievePinnedMessages()` → `MessageClient.GetPinnedMessagesAsync` |
| MS-F19 | Pin Message (deprecated) | `PUT /channels/{}/pins/{}` | Should | Deprecated | Still used: `IMessage.Pin()`, `ITextBasedChannel.PinMessageByIdAsync()` → `MessageClient.PinAsync` |
| MS-F20 | Unpin Message (deprecated) | `DELETE /channels/{}/pins/{}` | Should | Deprecated | Still used: `IMessage.Unpin()`, `ITextBasedChannel.UnpinMessageByIdAsync()` → `MessageClient.UnpinAsync` |
| MS-F21 | Message type `DEFAULT` (0) | `msgtype:0` | Should | Implemented | `MessageType.Default` |
| MS-F22 | Message type `RECIPIENT_ADD` (1) | `msgtype:1` | Should | Implemented | `MessageType.RecipientAdd` |
| MS-F23 | Message type `RECIPIENT_REMOVE` (2) | `msgtype:2` | Should | Implemented | `MessageType.RecipientRemove` |
| MS-F24 | Message type `CALL` (3) | `msgtype:3` | Should | Implemented | `MessageType.Call` |
| MS-F25 | Message type `CHANNEL_NAME_CHANGE` (4) | `msgtype:4` | Should | Implemented | `MessageType.ChannelNameChange` |
| MS-F26 | Message type `CHANNEL_ICON_CHANGE` (5) | `msgtype:5` | Should | Implemented | `MessageType.ChannelIconChange` |
| MS-F27 | Message type `CHANNEL_PINNED_MESSAGE` (6) | `msgtype:6` | Should | Implemented | `MessageType.ChannelPinnedMessage` |
| MS-F28 | Message type `USER_JOIN` (7) | `msgtype:7` | Should | Implemented | `MessageType.GuildMemberJoin` |
| MS-F29 | Message type `GUILD_BOOST` (8) | `msgtype:8` | Should | Implemented | `MessageType.UserPremiumGuildSubscription` |
| MS-F30 | Message type `GUILD_BOOST_TIER_1` (9) | `msgtype:9` | Should | Implemented | `MessageType.UserPremiumGuildSubscriptionTier1` |
| MS-F31 | Message type `GUILD_BOOST_TIER_2` (10) | `msgtype:10` | Should | Implemented | `MessageType.UserPremiumGuildSubscriptionTier2` |
| MS-F32 | Message type `GUILD_BOOST_TIER_3` (11) | `msgtype:11` | Should | Implemented | `MessageType.UserPremiumGuildSubscriptionTier3` |
| MS-F33 | Message type `CHANNEL_FOLLOW_ADD` (12) | `msgtype:12` | Should | Implemented | `MessageType.ChannelFollowAdd` |
| MS-F34 | Message type `GUILD_DISCOVERY_DISQUALIFIED` (14) | `msgtype:14` | Should | Implemented | `MessageType.GuildDiscoveryDisqualified` |
| MS-F35 | Message type `GUILD_DISCOVERY_REQUALIFIED` (15) | `msgtype:15` | Should | Implemented | `MessageType.GuildDiscoveryRequalified` |
| MS-F36 | Message type `GUILD_DISCOVERY_GRACE_PERIOD_INITIAL_WARNING` (16) | `msgtype:16` | Should | Implemented | `MessageType.GuildDiscoveryGracePeriodInitialWarning` |
| MS-F37 | Message type `GUILD_DISCOVERY_GRACE_PERIOD_FINAL_WARNING` (17) | `msgtype:17` | Should | Implemented | `MessageType.GuildDiscoveryGracePeriodFinalWarning` |
| MS-F38 | Message type `THREAD_CREATED` (18) | `msgtype:18` | Should | Implemented | `MessageType.ThreadCreated` |
| MS-F39 | Message type `REPLY` (19) | `msgtype:19` | Should | Implemented | `MessageType.Reply` |
| MS-F40 | Message type `CHAT_INPUT_COMMAND` (20) | `msgtype:20` | Should | Implemented | `MessageType.ChatInputCommand` |
| MS-F41 | Message type `THREAD_STARTER_MESSAGE` (21) | `msgtype:21` | Should | Implemented | `MessageType.ThreadStarterMessage` |
| MS-F42 | Message type `GUILD_INVITE_REMINDER` (22) | `msgtype:22` | Should | Implemented | `MessageType.GuildInviteReminder` |
| MS-F43 | Message type `CONTEXT_MENU_COMMAND` (23) | `msgtype:23` | Should | Implemented | `MessageType.ContextMenuCommand` |
| MS-F44 | Message type `AUTO_MODERATION_ACTION` (24) | `msgtype:24` | Should | Implemented | `MessageType.AutoModerationAction` |
| MS-F45 | Message type `ROLE_SUBSCRIPTION_PURCHASE` (25) | `msgtype:25` | Should | Implemented | `MessageType.RoleSubscriptionPurchase` |
| MS-F46 | Message type `INTERACTION_PREMIUM_UPSELL` (26) | `msgtype:26` | Should | Implemented | `MessageType.InteractionPremiumUpsell` |
| MS-F47 | Message type `STAGE_START` (27) | `msgtype:27` | Should | Implemented | `MessageType.StageStart` |
| MS-F48 | Message type `STAGE_END` (28) | `msgtype:28` | Should | Implemented | `MessageType.StageEnd` |
| MS-F49 | Message type `STAGE_SPEAKER` (29) | `msgtype:29` | Should | Implemented | `MessageType.StageSpeaker` |
| MS-F50 | Message type `STAGE_TOPIC` (31) | `msgtype:31` | Should | Implemented | `MessageType.StageTopic` |
| MS-F51 | Message type `GUILD_APPLICATION_PREMIUM_SUBSCRIPTION` (32) | `msgtype:32` | Should | Implemented | `MessageType.GuildApplicationPremiumSubscription` |
| MS-F52 | Message type `GUILD_INCIDENT_ALERT_MODE_ENABLED` (36) | `msgtype:36` | Should | Implemented | `MessageType.GuildIncidentAlertModeEnabled` |
| MS-F53 | Message type `GUILD_INCIDENT_ALERT_MODE_DISABLED` (37) | `msgtype:37` | Should | Implemented | `MessageType.GuildIncidentAlertModeDisabled` |
| MS-F54 | Message type `GUILD_INCIDENT_REPORT_RAID` (38) | `msgtype:38` | Should | Implemented | `MessageType.GuildIncidentReportRaid` |
| MS-F55 | Message type `GUILD_INCIDENT_REPORT_FALSE_ALARM` (39) | `msgtype:39` | Should | Implemented | `MessageType.GuildIncidentReportFalseAlarm` |
| MS-F56 | Message type `PURCHASE_NOTIFICATION` (44) | `msgtype:44` | Should | Missing | Not in `MessageType` |
| MS-F57 | Message type `POLL_RESULT` (46) | `msgtype:46` | Should | Missing | Not in `MessageType` |
| MS-F58 | Message object (content, author, embeds, attachments, components, flags, reference, snapshots, poll, stickers, thread, reactions) | — | Must | Implemented | `IMessage` |
| MS-F59 | Embeds with all fields and Discord limits validated | — | Must | Implemented | `EmbedBuilder`, `Embed` |
| MS-F60 | Allowed mentions (`parse`, `roles`, `users`, `replied_user`) | — | Must | Partial | `IMessageBuilderAction.SetAllowedMentions` (`MentionBuilder`) derives `parse`/`users`/`roles` from mentions written through the builder; an empty builder yields `null` (Discord default: ping everything), so "no pings" cannot be expressed, and `replied_user` is never set by the builder |
| MS-F61 | Replies (`message_reference`, `fail_if_not_exists`) | — | Must | Implemented | `IMessage.Reply()` → `MessageReference` |
| MS-F62 | Forwards (`message_reference.type = FORWARD`, `message_snapshots`) | — | Should | Implemented | `IMessage.ForwardTo()` (commit `9fe16f0`) |
| MS-F63 | Attachments: upload, keep/remove on edit, spoiler, description | — | Must | Implemented | `IMessageBuilderAction.AttachFile`, `IMessageBuilderAction.AttachFiles`, `IMessageBuilderAction.ClearAttachments`, `IMessage.ToBuilder()` |
| MS-F64 | Message flags on send (`SUPPRESS_EMBEDS`, `SUPPRESS_NOTIFICATIONS`, `EPHEMERAL` for interactions, `IS_COMPONENTS_V2`) | — | Must | Implemented | `IMessageBuilderAction.SetSuppressEmbeds`, `ICreateMessageBuilderBaseAction.SetSuppressNotifications`, `ISendMessageRestAction.SetEphemeral` |
| MS-F65 | Voice messages (`IS_VOICE_MESSAGE`, waveform, duration) | — | Could | Missing | `MessageFlags.IsVoiceMessage` is readable, but there is no way to send one |
| MS-F66 | Stickers in messages (`sticker_ids`) | — | Should | Implemented | `IBotMessageBuilderAction.SetStickers`, `IGuildTextChannelBase.SendStickers` |
| MS-F67 | TTS | — | Could | Implemented | `ICreateMessageBuilderBaseAction.SetTts` |
| MS-F68 | `nonce` / `enforce_nonce` de-duplication | — | Should | Missing | Fields exist on the request model, but no builder setter |
| MS-F69 | Reaction types (normal vs burst) when listing reactions | — | Should | Partial | `ITextBasedChannel.RetrieveReactionUsersById(…, ReactionType)` ignores the type (burst reactions are never requested) |
| MS-F70 | Purge helpers (bulk delete with the 14-day and 2–100 rules) | — | Should | Implemented | `ITextBasedChannel.PurgeMessagesAsync`, `ITextBasedChannel.PurgeMessagesByIdAsync` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| MS-N01 | Content length (2000) and required-body rules are validated before sending (`InvalidOperationException` / `ArgumentException`). |
| MS-N02 | Operations that Discord rejects on ephemeral messages (pin, reactions, crosspost) throw `EphemeralMessageException` locally. |
| MS-N03 | Multipart uploads rebuild streams on retry (SPEC-API-001). |

## 8. Configuration
None.

## 9. Compatibility
Moving pins to `/channels/{id}/messages/pins` does not change the public API (`IMessage.Pin()` and friends
stay the same). The new pin list is paginated, so `RetrievePinnedMessages()` should gain pagination.

## 10. Acceptance criteria
- [x] Send, edit, delete, crosspost, reactions and bulk delete (`MessageClientTests`, `MessageWrapperTests`, `TextBasedChannelWrapperTests`, `GetReactionsActionTests`, `ReactionWrapperTests`).
- [x] Embeds, mentions and text builders (`EmbedBuilderTests`, `MentionBuilderTests`, `MessageTextBuilderTests`).
- [x] Forward serialisation (`MessageForwardSerializationTests`).
- [ ] Pins use the current routes (MS-F15 – MS-F17).
- [ ] Guild message search (MS-F02).
- [ ] An explicit "no mentions" / `replied_user` option (MS-F60).

## 11. Open questions
- Should `GetMessages()` expose `IAsyncEnumerable<IMessage>`? Provisional: yes, as an extension over the
  pagination action.
