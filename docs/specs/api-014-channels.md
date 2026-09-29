# SPEC-API-014 — Channels and threads

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-014](../prd/api-014-channels.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/channel](https://docs.discord.com/developers/resources/channel), [topics/threads](https://docs.discord.com/developers/topics/threads) |
| **Last updated** | 2026-09-29 |

## 1. Summary
The wire `Channel` model is converted by `ChannelWrapper` (its "most specific type" factory) into
type-specific wrappers such as `GuildTextChannelWrapper`, `GuildForumChannelWrapper` and
`GuildThreadChannelWrapper`. Each implements the matching `DiscoSdk` interface. Edits go through channel
managers (`ChannelManagerWrapper<TSelf>` → `ManagerWrapper<TSelf>`, which tracks modified keys and
implements `IRestAction`). Thread creation and listing use `CreateThreadChannelAction` and
`ThreadChannelPaginationAction`. `ChannelClient` implements the routes, and `InviteClient` implements the
channel invite routes.

## 2. Projects and dependencies
- `DiscoSdk/Models/Channels/*`, the managers in `DiscoSdk/Rest/Actions`, and the enums `ChannelType`, `ChannelFlags`,
  `ThreadAutoArchiveDuration`, `ForumLayoutType`, `SortOrderType`, `VideoQualityMode`.
- `DiscoSdk.Hosting/Wrappers/Channels/*`, `Wrappers/Managers/*`, `Rest/Clients/ChannelClient.cs`,
  `Managers/ChannelManager.cs` (cache), `Containers/ChannelPermissionContainer.cs`.

## 3. Models and contracts
The wire `Channel` holds the union of all channel fields. Wrappers expose subsets through interfaces;
`IChannel` is the root, with `IGuildChannel`, `ITextBasedChannel`, `IThreadContainer` and others as
capability interfaces.

## 4. Components
| Type | Responsibility |
|---|---|
| `ChannelClient` | GET, PATCH and DELETE channel; permissions; followers; typing; group DM recipients; thread routes; archived lists. It also still calls the decommissioned active-threads route. |
| `ManagerWrapper<TSelf>` | Tracks modified keys and `Reset`; `ExecuteAsync` PATCHes only those keys. It implements `IRestAction`, but the public `IManager` interface does not expose it. |
| `CreateThreadChannelAction` | From a message (`POST …/messages/{id}/threads`) or as a forum/media post (`POST …/threads`). |
| `ThreadChannelPaginationAction` | Archived public/private lists (`before`, `limit`) and the active list (decommissioned route). |
| `ChannelManager` | Channel cache fed by gateway events. |

## 5. Public API
- `IDiscordClient.GetChannel(id)` / `GetChannel<T>(id)`.
- `IChannel.Delete()`, `IPermissionContainer.UpsertPermissionOverride` / `DeletePermissionOverride`.
- `IInviteContainer.CreateInvite` / `RetrieveInvites`, `IGuildNewsChannel.Follow()`.
- `IGroupDmChannel.AddRecipient` / `RemoveRecipient`.
- `IGuildThreadChannel.JoinThread` / `LeaveThread` / `AddThreadMember` / `RemoveThreadMember` /
  `ArchiveThread` / `LockThread`.
- `IThreadContainer.CreateThreadChannel` / `StartPost` / `GetThreadChannels` (forum and media only).
- `GetManager()` on each channel type.

## 6. Discord surface
There are 23 channel routes and 13 channel types at the baseline. `PUT /channels/{id}/voice-status` and
`GET /channels/{id}/users/@me/threads/archived/private` are not implemented.

## 7. Flows
**Edit**
1. `GetManager()`, then setters (each marks a key as modified).
2. `((IRestAction)manager).ExecuteAsync()` → `PATCH /channels/{id}` with the modified keys only.

**Forum post**
`StartPost(name)`, then the builder, then `POST /channels/{id}/threads` with a starter message.

## 8. Concurrency and lifecycle
Wrappers are snapshots of the cached model. Managers are single-use builders.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Manager with no changes | No request (`HasChanges == false`). |
| `GetThreadChannels()` on a forum with `archived: false` | **Current:** calls the decommissioned `/channels/{id}/threads/active` and fails. **Proposed:** filter `GET /guilds/{id}/threads/active` by parent. |
| Thread operations on an archived thread | Discord 400. The SDK does not unarchive implicitly. |

## 10. Observability
REST metrics; cache lookups (SPEC-SDK-04).

## 11. Tests
| Test class | Covers |
|---|---|
| `ChannelClientTests` | CH-F01 – CH-F22 |
| `ChannelWrapperTests`, `GuildChannelWrapperBaseTests`, `GuildTextBasedChannelWrapperTests`, `TextBasedChannelWrapperTests`, `GuildForumChannelWrapperTests`, `GuildMediaChannelWrapperTests`, `GuildThreadChannelWrapperTests`, `GuildStageChannelWrapperTests`, `GuildCategoryChannelWrapperTests`, `GuildNewsChannelWrapperTests`, `DmChannelWrapperTests`, `GroupDmChannelWrapperTests` | CH-F24 – CH-F37, CH-F39 – CH-F42 |
| `TextChannelManagerWrapperTests`, `ForumChannelManagerWrapperTests`, `VoiceChannelManagerWrapperTests`, `StageChannelManagerWrapperTests`, `ThreadChannelManagerWrapperTests`, `NewsChannelManagerWrapperTests`, `GuildChannelManagerWrapperTests` | CH-F02, CH-F38 |
| `ChannelPermissionContainerTests`, `OverridePermissionActionTests` | CH-F05, CH-F08 |
| `CreateGroupDmActionTests`, `OpenedDmsTests` | CH-F11, CH-F12 |

Implemented or Partial requirements without a covering test: CH-N01 – CH-N02.

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Permission overwrite edit/delete and group DM recipients. |
| `9f81c56` | Interface-first model split. |
| `82cbb28` | `ListActiveThreads` on the guild. |

Next steps:
1. Make `IManager<TSelf>` extend `IRestAction`.
2. Implement `IThreadContainer` on text and announcement channels.
3. Route active threads through the guild endpoint.
4. Expose thread members publicly.
5. Add voice status and joined private archived threads.

## 13. Decisions and rejected alternatives
- **Capability interfaces per channel type** rather than one `IChannel` with nullable members: invalid
  operations, such as setting a topic on a voice channel, fail at compile time.
