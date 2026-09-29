# PRD-API-014 — Channels and threads

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-014](../specs/api-014-channels.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/channel](https://docs.discord.com/developers/resources/channel), [topics/threads](https://docs.discord.com/developers/topics/threads) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Channels come in 13 types with different capabilities: text, announcement, voice, stage, category, forum,
media, the three thread types, DMs and group DMs. Bots create, edit and delete channels, manage permission
overwrites, run threads (create, archive, join, manage members) and publish announcements. A single
`Channel` class with every nullable field is error-prone. The SDK exposes type-specific interfaces instead.

## 2. Goals
- Type-specific channel interfaces with only the members valid for that type.
- Every channel and thread endpoint reachable from the public API.
- Edits go through fluent managers that send only changed fields.

## 3. Out of scope
- Messages, reactions and pins in channels: [PRD-API-015](api-015-messages.md).
- Channel creation and position changes (guild routes): [PRD-API-018](api-018-guilds.md).
- Permission computation: [PRD-API-006](api-006-permissions.md). Voice connections: [PRD-API-028](api-028-voice.md).

## 4. Usage scenarios
- As a bot author, I open a private thread off a text channel for each support ticket, and add the requester.
- I rename a channel and set its slowmode through its manager.
- I list archived public threads in a forum to clean up stale posts.

## 5. Desired developer experience

```csharp
var manager = textChannel.GetManager()
    .SetName("support")
    .SetTopic("Open a ticket with /ticket");
await ((IRestAction)manager).ExecuteAsync();   // cast needed today, see CH-F02

var post = await forum.StartPost("Bug: crash on start").ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| CH-F01 | Get Channel | `GET /channels/{}` | Must | Implemented | `IDiscordClient.GetChannel()` → `ChannelClient.GetAsync` (cache first, CH-N01) |
| CH-F02 | Modify Channel | `PATCH /channels/{}` | Must | Partial | Channel managers (`ITextChannelManager`, `IForumChannelManager`, …) build the PATCH, but `IManager` does not extend `IRestAction`, so callers must cast; the thread shortcuts `IGuildThreadChannel.ArchiveThread()` / `IGuildThreadChannel.LockThread()` work → `ChannelClient.EditAsync` |
| CH-F03 | Set Voice Channel Status | `PUT /channels/{}/voice-status` | Should | Missing | Voice channel status (PRD-API-028) |
| CH-F04 | Delete/Close Channel | `DELETE /channels/{}` | Must | Implemented | `IChannel.Delete()` → `ChannelClient.DeleteAsync` |
| CH-F05 | Edit Channel Permissions | `PUT /channels/{}/permissions/{}` | Must | Implemented | `IPermissionContainer.UpsertPermissionOverride()` → `ChannelClient.EditChannelPermissionsAsync` |
| CH-F06 | Get Channel Invites | `GET /channels/{}/invites` | Must | Implemented | `IInviteContainer.RetrieveInvites()` → `InviteClient.GetChannelInvitesAsync` |
| CH-F07 | Create Channel Invite | `POST /channels/{}/invites` | Must | Implemented | `IInviteContainer.CreateInvite()` → `InviteClient.CreateAsync` |
| CH-F08 | Delete Channel Permission | `DELETE /channels/{}/permissions/{}` | Must | Implemented | `IPermissionContainer.DeletePermissionOverride()` → `ChannelClient.DeleteChannelPermissionsAsync` |
| CH-F09 | Follow Announcement Channel | `POST /channels/{}/followers` | Must | Implemented | `IGuildNewsChannel.Follow()` → `ChannelClient.FollowAsync` |
| CH-F10 | Trigger Typing Indicator | `POST /channels/{}/typing` | Must | Implemented | `ITextBasedChannel.TriggerTypingAsync()` → `ChannelClient.TriggerTypingAsync` |
| CH-F11 | Group DM Add Recipient | `PUT /channels/{}/recipients/{}` | Must | Implemented | `IGroupDmChannel.AddRecipient()` → `ChannelClient.AddGroupDmRecipientAsync` |
| CH-F12 | Group DM Remove Recipient | `DELETE /channels/{}/recipients/{}` | Must | Implemented | `IGroupDmChannel.RemoveRecipient()` → `ChannelClient.RemoveGroupDmRecipientAsync` |
| CH-F13 | Start Thread from Message | `POST /channels/{}/messages/{}/threads` | Must | Partial | `IThreadContainer.CreateThreadChannel(name, messageId, isPrivate)` → `ChannelClient.CreateThreadFromMessageAsync`, but only forum and media channels implement `IThreadContainer`; text and announcement channels cannot start threads from a message |
| CH-F14 | Start Thread without Message / Start Thread in Forum or Media Channel | `POST /channels/{}/threads` | Must | Partial | `IThreadContainer.StartPost()` → `ChannelClient.CreateForumPostAsync` for forum and media channels; threads without a message in text or announcement channels are not reachable |
| CH-F15 | Join Thread | `PUT /channels/{}/thread-members/@me` | Must | Implemented | `IGuildThreadChannel.JoinThread()` → `ChannelClient.JoinThreadAsync` |
| CH-F16 | Add Thread Member | `PUT /channels/{}/thread-members/{}` | Must | Implemented | `IGuildThreadChannel.AddThreadMember()` → `ChannelClient.AddThreadMemberAsync` |
| CH-F17 | Leave Thread | `DELETE /channels/{}/thread-members/@me` | Must | Implemented | `IGuildThreadChannel.LeaveThread()` → `ChannelClient.LeaveThreadAsync` |
| CH-F18 | Remove Thread Member | `DELETE /channels/{}/thread-members/{}` | Must | Implemented | `IGuildThreadChannel.RemoveThreadMember()` → `ChannelClient.RemoveThreadMemberAsync` |
| CH-F19 | Get Thread Member | `GET /channels/{}/thread-members/{}` | Should | Partial | `ChannelClient.GetThreadMemberAsync` exists, but no public API calls it |
| CH-F20 | List Thread Members | `GET /channels/{}/thread-members` | Should | Partial | `ChannelClient.GetThreadMembersAsync` exists, but no public API calls it |
| CH-F21 | List Public Archived Threads | `GET /channels/{}/threads/archived/public` | Must | Implemented | `IThreadContainer.GetThreadChannels()` → `ChannelClient.GetPublicArchivedThreadsAsync` |
| CH-F22 | List Private Archived Threads | `GET /channels/{}/threads/archived/private` | Must | Implemented | `IThreadContainer.GetThreadChannels()` → `ChannelClient.GetPrivateArchivedThreadsAsync` |
| CH-F23 | List Joined Private Archived Threads | `GET /channels/{}/users/@me/threads/archived/private` | Should | Missing | — |
| CH-F24 | Channel type `GUILD_TEXT` (0) | `chtype:0` | Must | Implemented | `ChannelType.GuildText` |
| CH-F25 | Channel type `DM` (1) | `chtype:1` | Must | Implemented | `ChannelType.Dm` |
| CH-F26 | Channel type `GUILD_VOICE` (2) | `chtype:2` | Must | Implemented | `ChannelType.GuildVoice` |
| CH-F27 | Channel type `GROUP_DM` (3) | `chtype:3` | Must | Implemented | `ChannelType.GroupDm` |
| CH-F28 | Channel type `GUILD_CATEGORY` (4) | `chtype:4` | Must | Implemented | `ChannelType.GuildCategory` |
| CH-F29 | Channel type `GUILD_ANNOUNCEMENT` (5) | `chtype:5` | Must | Implemented | `ChannelType.GuildAnnouncement` |
| CH-F30 | Channel type `ANNOUNCEMENT_THREAD` (10) | `chtype:10` | Must | Implemented | `ChannelType.AnnouncementThread` |
| CH-F31 | Channel type `PUBLIC_THREAD` (11) | `chtype:11` | Must | Implemented | `ChannelType.PublicThread` |
| CH-F32 | Channel type `PRIVATE_THREAD` (12) | `chtype:12` | Must | Implemented | `ChannelType.PrivateThread` |
| CH-F33 | Channel type `GUILD_STAGE_VOICE` (13) | `chtype:13` | Must | Implemented | `ChannelType.GuildStageVoice` |
| CH-F34 | Channel type `GUILD_DIRECTORY` (14) | `chtype:14` | Must | Implemented | `ChannelType.GuildDirectory` |
| CH-F35 | Channel type `GUILD_FORUM` (15) | `chtype:15` | Must | Implemented | `ChannelType.GuildForum` |
| CH-F36 | Channel type `GUILD_MEDIA` (16) | `chtype:16` | Must | Implemented | `ChannelType.GuildMedia` |
| CH-F37 | Type-specific channel interfaces (text, news, voice, stage, category, forum, media, thread, DM, group DM) | — | Must | Implemented | `IGuildTextChannel`, `IGuildNewsChannel`, `IGuildVoiceChannel`, `IGuildStageChannel`, `IGuildCategoryChannel`, `IGuildForumChannel`, `IGuildMediaChannel`, `IGuildThreadChannel`, `IDmChannel`, `IGroupDmChannel` |
| CH-F38 | Channel managers send only modified fields (name, topic, NSFW, slowmode, bitrate, user limit, RTC region, video quality, parent, overwrites, forum settings) | — | Must | Partial | `ITextChannelManager`, `IForumChannelManager`, … → `ChannelManagerWrapper`. The public `IManager` contract has no `ExecuteAsync`; callers must cast to `IRestAction` (see CH-F02). |
| CH-F39 | Thread metadata and thread member objects | — | Must | Implemented | `IGuildThreadChannel` (archived, locked, auto-archive duration, invitable, member count) |
| CH-F40 | Forum tags, default reaction, sort order, layout | — | Should | Implemented | `IGuildForumChannel.AvailableTags`, `IGuildForumChannel.DefaultReactionEmoji`, `IGuildForumChannel.DefaultSortOrder`, `IGuildForumChannel.DefaultLayout` |
| CH-F41 | Channel flags (pinned, require tag, hide media download options) | — | Should | Implemented | `ChannelFlags` |
| CH-F43 | List active threads in a channel (decommissioned per the Discord change log; replaced by `GET /guilds/{id}/threads/active`) | `GET /channels/{}/threads/active` | Should | Deprecated | Not called: `IThreadContainer.GetThreadChannels()` lists `GET /guilds/{id}/threads/active` (GD-F07) and keeps the threads whose parent is the channel |
| CH-F42 | Thread auto-archive durations and thread limits (Discord threads topic) | — | Should | Implemented | `ThreadAutoArchiveDuration` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| CH-N01 | Channel reads prefer the cache (`ChannelManager`), and REST is used only on a miss (`IDiscordClient.GetChannel`). |
| CH-N02 | Every mutation accepts `WithReason`. |

## 8. Configuration
None.

## 9. Compatibility
- Making `IManager<TSelf>` extend `IRestAction` is source-compatible (additive for callers).
- Adding `IThreadContainer` to text and announcement channels is additive.

## 10. Acceptance criteria
- [x] Channel wrappers expose type-specific members (`ChannelWrapperTests`, `GuildTextBasedChannelWrapperTests`, `GuildForumChannelWrapperTests`, `GuildMediaChannelWrapperTests`, `GuildThreadChannelWrapperTests`, `GuildStageChannelWrapperTests`, `GuildCategoryChannelWrapperTests`, `GuildNewsChannelWrapperTests`, `DmChannelWrapperTests`, `GroupDmChannelWrapperTests`).
- [x] Managers patch only changed fields (`TextChannelManagerWrapperTests`, `ForumChannelManagerWrapperTests`, `VoiceChannelManagerWrapperTests`, `StageChannelManagerWrapperTests`, `ThreadChannelManagerWrapperTests`, `NewsChannelManagerWrapperTests`, `GuildChannelManagerWrapperTests`).
- [x] Thread and overwrite REST calls (`ChannelClientTests`, `OverridePermissionActionTests`).
- [ ] Managers are executable without a cast (CH-F02, CH-F38).
- [ ] Text and announcement channels can start threads, with or without a message (CH-F13, CH-F14).
- [ ] Thread members can be listed and fetched publicly (CH-F19, CH-F20).

## 11. Open questions
- Should `IManager<TSelf>` extend `IRestAction`, or get an `ApplyAsync()`? Provisional: extend
  `IRestAction`, which keeps the `ExecuteAsync` idiom used everywhere else.
