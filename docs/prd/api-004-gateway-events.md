# PRD-API-004 — Gateway events

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-004](../specs/api-004-gateway-events.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [events/gateway-events](https://docs.discord.com/developers/events/gateway-events) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Every Discord state change reaches a bot as a gateway dispatch. The SDK must expose each one as a typed,
intent-aware handler with a rich context (the entity plus helpers to act on it). It must also keep caches
coherent before handlers run.

## 2. Goals
- One handler interface per receive event (`IXxxHandler : IDiscordEventHandler<IXxxContext>`), registered
  through the builder or dependency injection.
- Contexts expose wrapped entities (`IMessage`, `IGuild`, …) and the originating shard.
- Caches (guilds, channels, members, presences, stickers) are updated before handlers are invoked.
- Intent requirements are declared on handlers and checked at registration.

## 3. Out of scope
- The send events (Identify, Resume, Heartbeat, Request Guild Members, Request Soundboard Sounds, Request
  Channel Info, Update Voice State, Update Presence) are opcodes and live in
  [PRD-API-003](api-003-gateway-connection.md).
- How handlers are executed (order, `[FireAndForget]`, dependency-injection scopes) is covered in
  [PRD-SDK-02](sdk-02-event-dispatch.md).
- The voice events' payload semantics are covered in [PRD-API-028](api-028-voice.md). They are still listed
  here as rows.

## 4. Usage scenarios
- As a bot author, when a member joins, `IGuildMemberAddHandler` receives the member and the guild, and can
  send a welcome message through `context`.
- When a message is deleted, `IMessageDeleteHandler` tells me the channel and message ID even if the
  message is not cached.
- When I register a presence handler without `GuildPresences`, I get a warning at registration.

## 5. Desired developer experience

```csharp
public sealed class Welcome : IGuildMemberAddHandler
{
    public async Task HandleAsync(IGuildMemberAddContext context, IServiceProvider services)
    {
        var channel = context.Guild.Channels.GetSystem();
        if (channel is null)
            return;

        await channel.SendMessage($"Welcome <@{context.Member.User.Id}>!").ExecuteAsync();
    }
}

var client = DiscordClientBuilder.Create(token)
    .WithIntents(DiscordIntent.Guilds | DiscordIntent.GuildMembers)
    .AddEventHandler<Welcome>()
    .Build();
```

## 6. Capabilities

| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| GE-F01 | `READY` | `event:READY` | Must | Implemented | `IDiscordClient.OnReady`, `IDiscordClient.WaitReadyAsync` → `Shard` (session, unavailable guilds); no per-shard handler interface |
| GE-F02 | `RESUMED` | `event:RESUMED` | Must | Implemented | `Shard` (restores `ShardStatus.Ready`; replayed events flow through the normal handlers); no handler interface |
| GE-F03 | `APPLICATION_COMMAND_PERMISSIONS_UPDATE` | `event:APPLICATION_COMMAND_PERMISSIONS_UPDATE` | Should | Missing | — |
| GE-F04 | `AUTO_MODERATION_RULE_CREATE` | `event:AUTO_MODERATION_RULE_CREATE` | Must | Implemented | `IAutoModerationRuleCreateHandler` |
| GE-F05 | `AUTO_MODERATION_RULE_UPDATE` | `event:AUTO_MODERATION_RULE_UPDATE` | Must | Implemented | `IAutoModerationRuleUpdateHandler` |
| GE-F06 | `AUTO_MODERATION_RULE_DELETE` | `event:AUTO_MODERATION_RULE_DELETE` | Must | Implemented | `IAutoModerationRuleDeleteHandler` |
| GE-F07 | `AUTO_MODERATION_ACTION_EXECUTION` | `event:AUTO_MODERATION_ACTION_EXECUTION` | Must | Implemented | `IAutoModerationActionExecutionHandler` |
| GE-F08 | `CHANNEL_CREATE` | `event:CHANNEL_CREATE` | Must | Implemented | `IChannelCreateHandler` |
| GE-F09 | `CHANNEL_UPDATE` | `event:CHANNEL_UPDATE` | Must | Implemented | `IChannelUpdateHandler` |
| GE-F10 | `CHANNEL_DELETE` | `event:CHANNEL_DELETE` | Must | Implemented | `IChannelDeleteHandler` |
| GE-F11 | `CHANNEL_INFO` | `event:CHANNEL_INFO` | Could | Missing | Needs op 43 (PRD-API-003) |
| GE-F12 | `VOICE_CHANNEL_STATUS_UPDATE` | `event:VOICE_CHANNEL_STATUS_UPDATE` | Should | Missing | — |
| GE-F13 | `VOICE_CHANNEL_START_TIME_UPDATE` | `event:VOICE_CHANNEL_START_TIME_UPDATE` | Could | Missing | — |
| GE-F14 | `THREAD_CREATE` | `event:THREAD_CREATE` | Must | Implemented | `IThreadCreateHandler` |
| GE-F15 | `THREAD_UPDATE` | `event:THREAD_UPDATE` | Must | Implemented | `IThreadUpdateHandler` |
| GE-F16 | `THREAD_DELETE` | `event:THREAD_DELETE` | Must | Implemented | `IThreadDeleteHandler` |
| GE-F17 | `THREAD_LIST_SYNC` | `event:THREAD_LIST_SYNC` | Must | Implemented | `IThreadListSyncHandler` |
| GE-F18 | `THREAD_MEMBER_UPDATE` | `event:THREAD_MEMBER_UPDATE` | Must | Implemented | `IThreadMemberUpdateHandler` |
| GE-F19 | `THREAD_MEMBERS_UPDATE` | `event:THREAD_MEMBERS_UPDATE` | Must | Implemented | `IThreadMembersUpdateHandler` |
| GE-F20 | `CHANNEL_PINS_UPDATE` | `event:CHANNEL_PINS_UPDATE` | Must | Implemented | `IChannelPinsUpdateHandler` |
| GE-F21 | `ENTITLEMENT_CREATE` | `event:ENTITLEMENT_CREATE` | Must | Implemented | `IEntitlementCreateHandler` |
| GE-F22 | `ENTITLEMENT_UPDATE` | `event:ENTITLEMENT_UPDATE` | Must | Implemented | `IEntitlementUpdateHandler` |
| GE-F23 | `ENTITLEMENT_DELETE` | `event:ENTITLEMENT_DELETE` | Must | Implemented | `IEntitlementDeleteHandler` |
| GE-F24 | `GUILD_CREATE` | `event:GUILD_CREATE` | Must | Implemented | `IGuildCreateHandler` |
| GE-F25 | `GUILD_UPDATE` | `event:GUILD_UPDATE` | Must | Implemented | `IGuildUpdateHandler` |
| GE-F26 | `GUILD_DELETE` | `event:GUILD_DELETE` | Must | Implemented | `IGuildDeleteHandler` |
| GE-F27 | `GUILD_AUDIT_LOG_ENTRY_CREATE` | `event:GUILD_AUDIT_LOG_ENTRY_CREATE` | Must | Implemented | `IAuditLogEntryCreateHandler` |
| GE-F28 | `GUILD_BAN_ADD` | `event:GUILD_BAN_ADD` | Must | Implemented | `IGuildBanAddHandler` |
| GE-F29 | `GUILD_BAN_REMOVE` | `event:GUILD_BAN_REMOVE` | Must | Implemented | `IGuildBanRemoveHandler` |
| GE-F30 | `GUILD_EMOJIS_UPDATE` | `event:GUILD_EMOJIS_UPDATE` | Must | Implemented | `IGuildEmojisUpdateHandler` |
| GE-F31 | `GUILD_STICKERS_UPDATE` | `event:GUILD_STICKERS_UPDATE` | Must | Implemented | `IGuildStickersUpdateHandler` |
| GE-F32 | `GUILD_INTEGRATIONS_UPDATE` | `event:GUILD_INTEGRATIONS_UPDATE` | Must | Implemented | `IGuildIntegrationsUpdateHandler` |
| GE-F33 | `GUILD_MEMBER_ADD` | `event:GUILD_MEMBER_ADD` | Must | Implemented | `IGuildMemberAddHandler` |
| GE-F34 | `GUILD_MEMBER_REMOVE` | `event:GUILD_MEMBER_REMOVE` | Must | Implemented | `IGuildMemberRemoveHandler` |
| GE-F35 | `GUILD_MEMBER_UPDATE` | `event:GUILD_MEMBER_UPDATE` | Must | Implemented | `IGuildMemberUpdateHandler` |
| GE-F36 | `GUILD_MEMBERS_CHUNK` | `event:GUILD_MEMBERS_CHUNK` | Must | Implemented | `IGuildMembersChunkHandler` |
| GE-F37 | `GUILD_ROLE_CREATE` | `event:GUILD_ROLE_CREATE` | Must | Implemented | `IGuildRoleCreateHandler` |
| GE-F38 | `GUILD_ROLE_UPDATE` | `event:GUILD_ROLE_UPDATE` | Must | Implemented | `IGuildRoleUpdateHandler` |
| GE-F39 | `GUILD_ROLE_DELETE` | `event:GUILD_ROLE_DELETE` | Must | Implemented | `IGuildRoleDeleteHandler` |
| GE-F40 | `GUILD_SCHEDULED_EVENT_CREATE` | `event:GUILD_SCHEDULED_EVENT_CREATE` | Must | Implemented | `IGuildScheduledEventCreateHandler` |
| GE-F41 | `GUILD_SCHEDULED_EVENT_UPDATE` | `event:GUILD_SCHEDULED_EVENT_UPDATE` | Must | Implemented | `IGuildScheduledEventUpdateHandler` |
| GE-F42 | `GUILD_SCHEDULED_EVENT_DELETE` | `event:GUILD_SCHEDULED_EVENT_DELETE` | Must | Implemented | `IGuildScheduledEventDeleteHandler` |
| GE-F43 | `GUILD_SCHEDULED_EVENT_USER_ADD` | `event:GUILD_SCHEDULED_EVENT_USER_ADD` | Must | Implemented | `IGuildScheduledEventUserAddHandler` |
| GE-F44 | `GUILD_SCHEDULED_EVENT_USER_REMOVE` | `event:GUILD_SCHEDULED_EVENT_USER_REMOVE` | Must | Implemented | `IGuildScheduledEventUserRemoveHandler` |
| GE-F45 | `GUILD_SOUNDBOARD_SOUND_CREATE` | `event:GUILD_SOUNDBOARD_SOUND_CREATE` | Should | Missing | Soundboard REST exists (PRD-API-024); events are not dispatched |
| GE-F46 | `GUILD_SOUNDBOARD_SOUND_UPDATE` | `event:GUILD_SOUNDBOARD_SOUND_UPDATE` | Should | Missing | Soundboard REST exists (PRD-API-024); events are not dispatched |
| GE-F47 | `GUILD_SOUNDBOARD_SOUND_DELETE` | `event:GUILD_SOUNDBOARD_SOUND_DELETE` | Should | Missing | Soundboard REST exists (PRD-API-024); events are not dispatched |
| GE-F48 | `GUILD_SOUNDBOARD_SOUNDS_UPDATE` | `event:GUILD_SOUNDBOARD_SOUNDS_UPDATE` | Should | Missing | Soundboard REST exists (PRD-API-024); events are not dispatched |
| GE-F49 | `SOUNDBOARD_SOUNDS` | `event:SOUNDBOARD_SOUNDS` | Could | Missing | Reply to op 31, which is not sent (PRD-API-003) |
| GE-F50 | `INTEGRATION_CREATE` | `event:INTEGRATION_CREATE` | Must | Implemented | `IIntegrationCreateHandler` |
| GE-F51 | `INTEGRATION_UPDATE` | `event:INTEGRATION_UPDATE` | Must | Implemented | `IIntegrationUpdateHandler` |
| GE-F52 | `INTEGRATION_DELETE` | `event:INTEGRATION_DELETE` | Must | Implemented | `IIntegrationDeleteHandler` |
| GE-F53 | `INVITE_CREATE` | `event:INVITE_CREATE` | Must | Implemented | `IInviteCreateHandler` |
| GE-F54 | `INVITE_DELETE` | `event:INVITE_DELETE` | Must | Implemented | `IInviteDeleteHandler` |
| GE-F55 | `MESSAGE_CREATE` | `event:MESSAGE_CREATE` | Must | Implemented | `IMessageCreateHandler` |
| GE-F56 | `MESSAGE_UPDATE` | `event:MESSAGE_UPDATE` | Must | Implemented | `IMessageUpdateHandler` |
| GE-F57 | `MESSAGE_DELETE` | `event:MESSAGE_DELETE` | Must | Implemented | `IMessageDeleteHandler` |
| GE-F58 | `MESSAGE_DELETE_BULK` | `event:MESSAGE_DELETE_BULK` | Must | Implemented | `IMessageDeleteBulkHandler` |
| GE-F59 | `MESSAGE_REACTION_ADD` | `event:MESSAGE_REACTION_ADD` | Must | Implemented | `IMessageReactionAddHandler` |
| GE-F60 | `MESSAGE_REACTION_REMOVE` | `event:MESSAGE_REACTION_REMOVE` | Must | Implemented | `IMessageReactionRemoveHandler` |
| GE-F61 | `MESSAGE_REACTION_REMOVE_ALL` | `event:MESSAGE_REACTION_REMOVE_ALL` | Must | Implemented | `IMessageReactionRemoveAllHandler` |
| GE-F62 | `MESSAGE_REACTION_REMOVE_EMOJI` | `event:MESSAGE_REACTION_REMOVE_EMOJI` | Must | Implemented | `IMessageReactionRemoveEmojiHandler` |
| GE-F63 | `PRESENCE_UPDATE` | `event:PRESENCE_UPDATE` | Must | Implemented | `IPresenceUpdateHandler` |
| GE-F64 | `TYPING_START` | `event:TYPING_START` | Must | Implemented | `ITypingStartHandler` |
| GE-F65 | `USER_UPDATE` | `event:USER_UPDATE` | Must | Implemented | `IUserUpdateHandler` |
| GE-F66 | `VOICE_CHANNEL_EFFECT_SEND` | `event:VOICE_CHANNEL_EFFECT_SEND` | Could | Missing | Voice roadmap (PRD-API-028) |
| GE-F67 | `VOICE_STATE_UPDATE` | `event:VOICE_STATE_UPDATE` | Must | Missing | Voice roadmap (PRD-API-028); also needed for voice-state caches and `VoicePolicy` member caching |
| GE-F68 | `VOICE_SERVER_UPDATE` | `event:VOICE_SERVER_UPDATE` | Should | Missing | Voice roadmap (PRD-API-028) |
| GE-F69 | `WEBHOOKS_UPDATE` | `event:WEBHOOKS_UPDATE` | Must | Implemented | `IWebhooksUpdateHandler` |
| GE-F70 | `INTERACTION_CREATE` | `event:INTERACTION_CREATE` | Must | Implemented | `IInteractionCreateHandler`, `IApplicationCommandHandler`, `IUserCommandHandler`, `IMessageCommandHandler`, `IAutoCompleteHandler`, `IComponentInteractionHandler`, `IModalSubmitHandler` |
| GE-F71 | `STAGE_INSTANCE_CREATE` | `event:STAGE_INSTANCE_CREATE` | Must | Implemented | `IStageInstanceCreateHandler` |
| GE-F72 | `STAGE_INSTANCE_UPDATE` | `event:STAGE_INSTANCE_UPDATE` | Must | Implemented | `IStageInstanceUpdateHandler` |
| GE-F73 | `STAGE_INSTANCE_DELETE` | `event:STAGE_INSTANCE_DELETE` | Must | Implemented | `IStageInstanceDeleteHandler` |
| GE-F74 | `SUBSCRIPTION_CREATE` | `event:SUBSCRIPTION_CREATE` | Must | Implemented | `ISubscriptionCreateHandler` |
| GE-F75 | `SUBSCRIPTION_UPDATE` | `event:SUBSCRIPTION_UPDATE` | Must | Implemented | `ISubscriptionUpdateHandler` |
| GE-F76 | `SUBSCRIPTION_DELETE` | `event:SUBSCRIPTION_DELETE` | Must | Implemented | `ISubscriptionDeleteHandler` |
| GE-F77 | `MESSAGE_POLL_VOTE_ADD` | `event:MESSAGE_POLL_VOTE_ADD` | Must | Implemented | `IMessagePollVoteAddHandler` |
| GE-F78 | `MESSAGE_POLL_VOTE_REMOVE` | `event:MESSAGE_POLL_VOTE_REMOVE` | Must | Implemented | `IMessagePollVoteRemoveHandler` |
| GE-F79 | `RATE_LIMITED` | `event:RATE_LIMITED` | Should | Missing | — |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| GE-N01 | Payloads are parsed once from `JsonElement`. Contexts wrap models lazily and are never re-serialised. |
| GE-N02 | Unknown event names are ignored, so new Discord events never crash the dispatcher. Today they are dropped silently: the `switch` has no `default` branch and nothing is logged. |
| GE-N03 | Every handler interface carries `[RequiresIntent]` when Discord gates the event behind an intent. |
| GE-N04 | Exceptions thrown while processing an event never kill the shard. They are logged at `Error` ("Error processing event {EventType}"), and shard-level failures surface through `IDiscordClient.UnhandledError` (PRD-SDK-02). |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Handler registration | none | any `IDiscordEventHandler` | `AddEventHandler<T>()`, `AddEventHandler(Type)`, `AddEventHandler(instance)` |
| Intents | required | `DiscordIntent` | `WithIntents` |

## 9. Compatibility
- A new handler interface is additive.
- Renaming a context member is breaking, and must go through `[Obsolete]` for one minor release.

## 10. Acceptance criteria
- [x] Each implemented event dispatches to its handler with a populated context (`GuildDispatchTests`, `MessageDispatchTests`, `ChannelDispatchTests`, `ThreadDispatchTests`, `MemberDispatchTests`, `RoleDispatchTests`, `BanDispatchTests`, `ReactionDispatchTests`, `InviteDispatchTests`, `IntegrationDispatchTests`, `AutoModerationDispatchTests`, `EntitlementDispatchTests`, `SubscriptionDispatchTests`, `StageInstanceDispatchTests`, `GuildScheduledEventDispatchTests`, `PollVoteDispatchTests`, `TypingDispatchTests`, `UserPinsWebhooksDispatchTests`, `GuildExtrasDispatchTests`, `InteractionDispatchTests`).
- [x] Handlers without their required intent are reported at registration (`EventHandlerIntentGuardTests`).
- [ ] Handlers exist for the voice, soundboard, channel-info, rate-limited and command-permission events (GE-F03, F11–F13, F45–F49, F66–F68, F79).

## 11. Open questions
- Should `READY` and `RESUMED` get handler interfaces, or stay covered by `IDiscordClient.OnReady` and
  `WaitReadyAsync`? Provisional: add `IReadyHandler` carrying the shard, keeping `OnReady` for the
  all-shards-ready signal.
- `RATE_LIMITED` (opcode-level rate limit on op 8): surface it as a handler or only as a log and metric?
  Provisional: metric plus `Warning` log, with no public handler.
