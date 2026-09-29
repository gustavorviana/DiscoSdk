# PRD-API-006 — Permissions and roles model

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-006](../specs/api-006-permissions.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [topics/permissions](https://docs.discord.com/developers/topics/permissions) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Bots must check permissions before acting, both to avoid 403s (which count toward the Cloudflare invalid-request
budget, PRD-API-002) and to decide what to offer users. Discord defines 52 permission bits, a role hierarchy,
channel overwrites resolved in a specific order, implicit permissions and timeout restrictions. A
calculator that deviates from that algorithm gives wrong answers.

## 2. Goals
- A `DiscordPermission` flags enum with every documented bit.
- `IGuildChannelBase.GetPermission(IMember)` returns exactly what Discord computes.
- The role object is exposed completely: colors, tags, flags, icon.

## 3. Out of scope
- Role CRUD endpoints: [PRD-API-018](api-018-guilds.md).
- Channel overwrite endpoints: [PRD-API-014](api-014-channels.md).
- Application command permissions: [PRD-API-008](api-008-application-commands.md).

## 4. Usage scenarios
- As a bot author, before deleting messages I check `channel.GetPermission(me).HasFlag(ManageMessages)`.
- When a member has one role that denies `SEND_MESSAGES` and another that allows it in the same channel, the
  calculator returns *allowed*, as Discord does.

## 5. Desired developer experience

```csharp
var perms = channel.GetPermission(member);
if (!perms.HasFlag(DiscordPermission.ManageMessages))
    return;
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| PM-F01 | Base permissions: owner → all; @everyone role ∪ member roles; `ADMINISTRATOR` short-circuit | — | Must | Implemented | `IGuildChannelBase.GetPermission()` → `ChannelPermissionCalculator` |
| PM-F02 | Overwrite order: @everyone overwrite → role overwrites (all denies, then all allows) → member overwrite | — | Must | Partial | `ChannelPermissionCalculator` applies role overwrites one at a time by role position (allow, then deny), so a lower role's deny can beat a higher role's allow. The @everyone overwrite is only applied if `IMember.Roles` contains @everyone (it usually does not). |
| PM-F03 | Implicit permissions (no `VIEW_CHANNEL` → nothing; no `SEND_MESSAGES` → no mention/attach/embed) | — | Should | Missing | Not applied by `ChannelPermissionCalculator` |
| PM-F04 | Timed-out members keep only `VIEW_CHANNEL` and `READ_MESSAGE_HISTORY` | — | Should | Missing | Not applied; `IMember.TimeoutForAsync` exists but the calculator ignores the timeout |
| PM-F05 | Thread permissions inherit from the parent channel | — | Should | Partial | `ChannelWrapper.GetPermissionContainer` wraps the thread's own channel model, which has no overwrites, so a thread resolves base permissions only and ignores the parent's overwrites |
| PM-F06 | Role hierarchy (position comparisons for manage/kick/ban) | — | Should | Partial | `IRole : IComparable<IRole>` (position); no helper that tells whether a member can act on another |
| PM-F07 | Role object: id, name, permissions, position, hoist, managed, mentionable, icon, unicode emoji | — | Must | Implemented | `IRole` |
| PM-F08 | Role colors object (`primary_color`, `secondary_color`, `tertiary_color`) | — | Should | Partial | `IRole.Colors` is a single `Color`; gradients are exposed only on `IMember.Colors` (`RoleColors`) |
| PM-F09 | Role tags (bot, integration, premium subscriber, subscription listing, purchasable, guild connections) | — | Should | Implemented | `IRole.Tags` (`IRoleTags`) |
| PM-F10 | Role flags (`IN_PROMPT`) | — | Could | Partial | `RoleFlags.InPrompt` exists on the wire model (`Role.Flags`) but is not exposed on `IRole` |
| PM-F11 | Permission serialisation as a string bitfield | — | Must | Implemented | `DiscordPermissionConverter` |
| PM-F12 | Permission `CREATE_INSTANT_INVITE` (1 << 0) | `perm:CREATE_INSTANT_INVITE` | Must | Implemented | `DiscordPermission.CreateInstantInvite` |
| PM-F13 | Permission `KICK_MEMBERS` (1 << 1) | `perm:KICK_MEMBERS` | Must | Implemented | `DiscordPermission.KickMembers` |
| PM-F14 | Permission `BAN_MEMBERS` (1 << 2) | `perm:BAN_MEMBERS` | Must | Implemented | `DiscordPermission.BanMembers` |
| PM-F15 | Permission `ADMINISTRATOR` (1 << 3) | `perm:ADMINISTRATOR` | Must | Implemented | `DiscordPermission.Administrator` |
| PM-F16 | Permission `MANAGE_CHANNELS` (1 << 4) | `perm:MANAGE_CHANNELS` | Must | Implemented | `DiscordPermission.ManageChannels` |
| PM-F17 | Permission `MANAGE_GUILD` (1 << 5) | `perm:MANAGE_GUILD` | Must | Implemented | `DiscordPermission.ManageGuild` |
| PM-F18 | Permission `ADD_REACTIONS` (1 << 6) | `perm:ADD_REACTIONS` | Must | Implemented | `DiscordPermission.AddReactions` |
| PM-F19 | Permission `VIEW_AUDIT_LOG` (1 << 7) | `perm:VIEW_AUDIT_LOG` | Must | Implemented | `DiscordPermission.ViewAuditLog` |
| PM-F20 | Permission `PRIORITY_SPEAKER` (1 << 8) | `perm:PRIORITY_SPEAKER` | Must | Implemented | `DiscordPermission.PrioritySpeaker` |
| PM-F21 | Permission `STREAM` (1 << 9) | `perm:STREAM` | Must | Implemented | `DiscordPermission.Stream` |
| PM-F22 | Permission `VIEW_CHANNEL` (1 << 10) | `perm:VIEW_CHANNEL` | Must | Implemented | `DiscordPermission.ViewChannel` |
| PM-F23 | Permission `SEND_MESSAGES` (1 << 11) | `perm:SEND_MESSAGES` | Must | Implemented | `DiscordPermission.SendMessages` |
| PM-F24 | Permission `SEND_TTS_MESSAGES` (1 << 12) | `perm:SEND_TTS_MESSAGES` | Must | Implemented | `DiscordPermission.SendTtsMessages` |
| PM-F25 | Permission `MANAGE_MESSAGES` (1 << 13) | `perm:MANAGE_MESSAGES` | Must | Implemented | `DiscordPermission.ManageMessages` |
| PM-F26 | Permission `EMBED_LINKS` (1 << 14) | `perm:EMBED_LINKS` | Must | Implemented | `DiscordPermission.EmbedLinks` |
| PM-F27 | Permission `ATTACH_FILES` (1 << 15) | `perm:ATTACH_FILES` | Must | Implemented | `DiscordPermission.AttachFiles` |
| PM-F28 | Permission `READ_MESSAGE_HISTORY` (1 << 16) | `perm:READ_MESSAGE_HISTORY` | Must | Implemented | `DiscordPermission.ReadMessageHistory` |
| PM-F29 | Permission `MENTION_EVERYONE` (1 << 17) | `perm:MENTION_EVERYONE` | Must | Implemented | `DiscordPermission.MentionEveryone` |
| PM-F30 | Permission `USE_EXTERNAL_EMOJIS` (1 << 18) | `perm:USE_EXTERNAL_EMOJIS` | Must | Implemented | `DiscordPermission.UseExternalEmojis` |
| PM-F31 | Permission `VIEW_GUILD_INSIGHTS` (1 << 19) | `perm:VIEW_GUILD_INSIGHTS` | Must | Implemented | `DiscordPermission.ViewGuildInsights` |
| PM-F32 | Permission `CONNECT` (1 << 20) | `perm:CONNECT` | Must | Implemented | `DiscordPermission.Connect` |
| PM-F33 | Permission `SPEAK` (1 << 21) | `perm:SPEAK` | Must | Implemented | `DiscordPermission.Speak` |
| PM-F34 | Permission `MUTE_MEMBERS` (1 << 22) | `perm:MUTE_MEMBERS` | Must | Implemented | `DiscordPermission.MuteMembers` |
| PM-F35 | Permission `DEAFEN_MEMBERS` (1 << 23) | `perm:DEAFEN_MEMBERS` | Must | Implemented | `DiscordPermission.DeafenMembers` |
| PM-F36 | Permission `MOVE_MEMBERS` (1 << 24) | `perm:MOVE_MEMBERS` | Must | Implemented | `DiscordPermission.MoveMembers` |
| PM-F37 | Permission `USE_VAD` (1 << 25) | `perm:USE_VAD` | Must | Implemented | `DiscordPermission.UseVad` |
| PM-F38 | Permission `CHANGE_NICKNAME` (1 << 26) | `perm:CHANGE_NICKNAME` | Must | Implemented | `DiscordPermission.ChangeNickname` |
| PM-F39 | Permission `MANAGE_NICKNAMES` (1 << 27) | `perm:MANAGE_NICKNAMES` | Must | Implemented | `DiscordPermission.ManageNicknames` |
| PM-F40 | Permission `MANAGE_ROLES` (1 << 28) | `perm:MANAGE_ROLES` | Must | Implemented | `DiscordPermission.ManageRoles` |
| PM-F41 | Permission `MANAGE_WEBHOOKS` (1 << 29) | `perm:MANAGE_WEBHOOKS` | Must | Implemented | `DiscordPermission.ManageWebhooks` |
| PM-F42 | Permission `MANAGE_GUILD_EXPRESSIONS` (1 << 30) | `perm:MANAGE_GUILD_EXPRESSIONS` | Must | Implemented | `DiscordPermission.ManageEmojisAndStickers` |
| PM-F43 | Permission `USE_APPLICATION_COMMANDS` (1 << 31) | `perm:USE_APPLICATION_COMMANDS` | Must | Implemented | `DiscordPermission.UseApplicationCommands` |
| PM-F44 | Permission `REQUEST_TO_SPEAK` (1 << 32) | `perm:REQUEST_TO_SPEAK` | Must | Implemented | `DiscordPermission.RequestToSpeak` |
| PM-F45 | Permission `MANAGE_EVENTS` (1 << 33) | `perm:MANAGE_EVENTS` | Must | Implemented | `DiscordPermission.ManageEvents` |
| PM-F46 | Permission `MANAGE_THREADS` (1 << 34) | `perm:MANAGE_THREADS` | Must | Implemented | `DiscordPermission.ManageThreads` |
| PM-F47 | Permission `CREATE_PUBLIC_THREADS` (1 << 35) | `perm:CREATE_PUBLIC_THREADS` | Must | Implemented | `DiscordPermission.CreatePublicThreads` |
| PM-F48 | Permission `CREATE_PRIVATE_THREADS` (1 << 36) | `perm:CREATE_PRIVATE_THREADS` | Must | Implemented | `DiscordPermission.CreatePrivateThreads` |
| PM-F49 | Permission `USE_EXTERNAL_STICKERS` (1 << 37) | `perm:USE_EXTERNAL_STICKERS` | Must | Implemented | `DiscordPermission.UseExternalStickers` |
| PM-F50 | Permission `SEND_MESSAGES_IN_THREADS` (1 << 38) | `perm:SEND_MESSAGES_IN_THREADS` | Must | Implemented | `DiscordPermission.SendMessagesInThreads` |
| PM-F51 | Permission `USE_EMBEDDED_ACTIVITIES` (1 << 39) | `perm:USE_EMBEDDED_ACTIVITIES` | Must | Implemented | `DiscordPermission.UseEmbeddedActivities` |
| PM-F52 | Permission `MODERATE_MEMBERS` (1 << 40) | `perm:MODERATE_MEMBERS` | Must | Implemented | `DiscordPermission.ModerateMembers` |
| PM-F53 | Permission `VIEW_CREATOR_MONETIZATION_ANALYTICS` (1 << 41) | `perm:VIEW_CREATOR_MONETIZATION_ANALYTICS` | Must | Implemented | `DiscordPermission.ViewCreatorMonetizationAnalytics` |
| PM-F54 | Permission `USE_SOUNDBOARD` (1 << 42) | `perm:USE_SOUNDBOARD` | Must | Implemented | `DiscordPermission.UseSoundboard` |
| PM-F55 | Permission `CREATE_GUILD_EXPRESSIONS` (1 << 43) | `perm:CREATE_GUILD_EXPRESSIONS` | Must | Implemented | `DiscordPermission.CreateGuildExpressions` |
| PM-F56 | Permission `CREATE_EVENTS` (1 << 44) | `perm:CREATE_EVENTS` | Must | Implemented | `DiscordPermission.CreateEvents` |
| PM-F57 | Permission `USE_EXTERNAL_SOUNDS` (1 << 45) | `perm:USE_EXTERNAL_SOUNDS` | Must | Implemented | `DiscordPermission.UseExternalSounds` |
| PM-F58 | Permission `SEND_VOICE_MESSAGES` (1 << 46) | `perm:SEND_VOICE_MESSAGES` | Must | Implemented | `DiscordPermission.SendVoiceMessages` |
| PM-F59 | Permission `SET_VOICE_CHANNEL_STATUS` (1 << 48) | `perm:SET_VOICE_CHANNEL_STATUS` | Must | Missing | Not in `DiscordPermission` |
| PM-F60 | Permission `SEND_POLLS` (1 << 49) | `perm:SEND_POLLS` | Must | Missing | Not in `DiscordPermission` |
| PM-F61 | Permission `USE_EXTERNAL_APPS` (1 << 50) | `perm:USE_EXTERNAL_APPS` | Must | Missing | Not in `DiscordPermission` |
| PM-F62 | Permission `PIN_MESSAGES` (1 << 51) | `perm:PIN_MESSAGES` | Must | Missing | Not in `DiscordPermission` |
| PM-F63 | Permission `BYPASS_SLOWMODE` (1 << 52) | `perm:BYPASS_SLOWMODE` | Must | Missing | Not in `DiscordPermission` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| PM-N01 | Permission computation is allocation-free except for the role lookup, and uses cached roles only (no REST). |
| PM-N02 | `DiscordPermission` is a `ulong` flags enum, so bits above 31 are represented. |

## 8. Configuration
None.

## 9. Compatibility
Fixing PM-F02 changes computed results, which is a behavioural fix and not an API break. Changing
`IRole.Colors` to `RoleColors` is breaking. Add `IRole.ColorSet` and obsolete the old member instead.

## 10. Acceptance criteria
- [x] Overwrites and base permissions resolve for common cases (`ChannelPermissionContainerTests`, `OverridePermissionActionTests`).
- [x] Permission bitfields round-trip as strings (`DiscordPermissionConverterTests`).
- [ ] Discord's documented examples pass, including the @everyone overwrite and cross-role allow/deny (PM-F02).
- [ ] The implicit permission and timeout rules hold (PM-F03, PM-F04).
- [ ] `DiscordPermission` has bits 48–52 (PM-F59 – PM-F63).

## 11. Open questions
- Should `GetPermission` be available without a cached guild (for example, when only the interaction's
  `app_permissions` is available)? Provisional: expose `IInteraction.AppPermissions` for interactions,
  and keep the calculator cache-based.
