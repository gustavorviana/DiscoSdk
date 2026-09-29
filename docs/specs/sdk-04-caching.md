# SPEC-SDK-04 — Caching and managers

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-SDK-04](../prd/sdk-04-caching.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — |
| **Last updated** | 2026-09-29 |

## 1. Summary
`DiscordClient` builds one manager per entity when it is constructed:
- `GuildManager` for guilds;
- `ChannelManager` for channels;
- `UserRepository` for users;
- `DmChannelRepository` for DM channels;
- `MemberManager` for members, using the `IMemberCachePolicy` from the container, or `AllPolicy` when none
  is registered;
- `PresenceManager` for presences, using `PresenceCacheConfiguration`, default `ClientStatus`;
- `StickerManager` for stickers, using `StickerCacheConfiguration`, default `None`.

`DiscordEventDispatcher` feeds them before it invokes the handlers (SPEC-API-004), so a handler always sees
the updated state. Every store is a `ConcurrentDictionary`, either directly or through `SnowflakeCollection`.
The member, presence and sticker stores are partitioned by guild id and then by entity id.

## 2. Projects and dependencies
- `DiscoSdk/Caching/*`: `IMemberCachePolicy`, `MemberCachePolicy`, `PolicyMode`, `MemberFetchMode`,
  `PresenceCacheFlag`, `StickerCacheFlag`, `StickerFetchMode`, `IMemberManager`, `IGuildMembers`,
  `IStickerManager`, `IGuildStickers`.
- `DiscoSdk.Hosting/Caching/*`: `MemberCachePolicyBuilder`, `MemberCachePolicyExtensions`, `Policies/*`,
  `PresenceCacheConfiguration`, `StickerCacheConfiguration`.
- `DiscoSdk.Hosting/Managers/*`, `Repositories/*`, `Utils/SnowflakeCollection.cs`.

## 3. Models and contracts
- The caches store wire models (`GuildMember`, `Presence`, `Sticker`) and wrap them on read. Guilds and
  channels are stored as wrappers and mutated in place on update (`ChannelWrapper.OnUpdate`).
- `IMemberCachePolicy.ShouldCache(IMember)` receives a wrapped member, so `OnlineStatus` reads from the
  presence cache.
- The composite policies are `GroupPolicy` (All = AND, Any = OR) and `PredicatePolicy`. The leaf policies
  are `OwnerPolicy`, `VoicePolicy`, `OnlinePolicy`, `BoosterPolicy`, `PendingPolicy`, `RolesPolicy`,
  `AllPolicy` and `NonePolicy`.

## 4. Components
| Type | Responsibility |
|---|---|
| `GuildManager` | Guild store; pending-guild set from READY; channel create/update/delete routing; `GetAsync` / `TryGet` / `All` warn once without the `Guilds` intent. |
| `ChannelManager` | Channel store; `Get` with REST fallback and write-back. |
| `UserRepository` | User store; `Get` with REST fallback; `Upsert` from events. |
| `MemberManager` | Member store; policy evaluation on seed, add/update and chunk; removal on member remove and guild remove; `Get` with fetch mode; `OfGuild` → `GuildMembersImpl` (cache reads plus REST builders plus op 8). |
| `PresenceManager` | Presence store filtered by `PresenceCacheFlag`; `MapStatus` string → `OnlineStatus`. |
| `StickerManager` | Guild sticker store (only when `StickerCacheFlag.Guild` is set); `GUILD_STICKERS_UPDATE` replaces the guild's set; `Get` / `GetAll` with fetch mode. |
| `GuildCacheWarnTracker` | Process-wide, one-shot missing-intent warning. |

## 5. Public API
- `IDiscordClient.Members` (`IMemberManager`) and `IDiscordClient.Stickers` (`IStickerManager`).
- `IGuild.Members` (`IGuildMembers`) and `IGuild.Stickers` (`IGuildStickers`).
- Builder: `WithMemberCachePolicy` (five overloads), `WithPresenceCache`, `WithStickerCache`.

## 6. Discord surface
- **Feeding events:** `READY`, `GUILD_CREATE`, `GUILD_UPDATE`, `GUILD_DELETE`, `CHANNEL_*`,
  `GUILD_MEMBER_ADD` / `GUILD_MEMBER_UPDATE` / `GUILD_MEMBER_REMOVE`, `GUILD_MEMBERS_CHUNK`,
  `PRESENCE_UPDATE` and `GUILD_STICKERS_UPDATE`.
- **Fallback routes:**
  - `GET /channels/{}`;
  - `GET /users/{}`;
  - `GET /guilds/{}/members/{}`;
  - `GET /guilds/{}/stickers` and `GET /guilds/{}/stickers/{}`.
- **Intents:**
  - `GUILDS` for guilds and channels;
  - `GUILD_MEMBERS` for member events and full chunks;
  - `GUILD_PRESENCES` for presences and the members in `GUILD_CREATE` on large guilds;
  - `GUILD_EXPRESSIONS` for sticker updates.

## 7. Flows
**`GUILD_CREATE`**
All of these steps run under the `GuildManager` lock:
1. `GuildManager.HandleGuildCreate` stores the guild wrapper.
2. `PresenceManager.OnGuildPresencesSeed` runs first, filtered by the presence flags, so that policies can
   read presences.
3. `MemberManager.OnGuildMembersSeed` evaluates the policy for each member.
4. `StickerManager.OnGuildStickersSeed` runs, and is a no-op when the sticker cache is off.
5. The member and sticker arrays are dropped from the guild model, so the managers are the single source.
6. The guild is removed from the pending set. When the last pending guild arrives, `Information` is
   logged ("fully initialized").

**`GUILD_MEMBER_UPDATE`**
Wrap the member, then call `ShouldCache`. If it returns true, upsert the member; otherwise remove it.

**`PRESENCE_UPDATE`**
`PresenceManager.OnPresenceUpdate` stores the presence. The member policy is **not** re-evaluated
(CA-F08).

**Lookup (`CacheThenRest`)**
1. On a cache hit, record `hit` and return.
2. On a miss, record `miss` and call REST.
3. If the member is found and the policy accepts it, write it back, then record `rest`.
4. A REST exception is logged at `Warning` and the lookup returns `null`.

## 8. Concurrency and lifecycle
- Writes come from shard workers. Guilds are partitioned by shard, so there is no cross-shard write to the
  same guild partition.
- Reads come from any thread. Enumeration (`GetCached`) is a snapshot.
- Caches live for the client's lifetime and are only cleared per guild on `GUILD_DELETE` (unavailable or
  removed).

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| `MemberCachePolicy.Online` | Caches members whose presence is Online, Idle or DoNotDisturb (`is not (Offline or Invisible)`); needs `GUILD_PRESENCES` and the `ClientStatus` presence flag. |
| `MemberCachePolicy.Voice` | Never matches: `IMember.VoiceState` is always null (SPEC-API-028). |
| Reading the guild cache without the `Guilds` intent | A single `Warning`; lookups return empty or null. |
| REST fallback fails | `Warning` logged; the result is `null` (no exception). |
| Large guild without `GUILD_PRESENCES` | `GUILD_CREATE` carries only a subset of members; use op 8 (`IGuildMembers.Request`) to fill the cache. |

## 10. Observability
- `DiscoSdkDiagnostics.CacheLookups`, tagged with the result (`hit` / `miss` / `rest`) and the entity
  (member, sticker).
- `DiscoSdkDiagnostics.CacheEvictions` is declared but never recorded (CA-F15).

## 11. Tests
| Test class | Covers |
|---|---|
| `PolicyPresetsTests` | CA-F05, CA-N02 |
| `MemberCachePolicyBuilderTests` | CA-F06, CA-F07 |
| `MemberManagerSeedTests`, `MemberCacheDispatchTests` | CA-F01, CA-F04, CA-F09, CA-F12, CA-N03 |
| `PresenceManagerTests` | CA-F10 |
| `StickerCacheDispatchTests` | CA-F11 |
| `CacheLookupsMetricTests` | CA-F14 |
| `GuildCacheIntentGuardTests` | CA-F13 |
| `TextChannelManagerWrapperTests` and the other channel manager wrapper tests | CA-F02 (channel edits) |

Implemented or Partial requirements without a covering test: CA-F03, CA-F15, CA-N01.

## 12. History
| Commit | Change |
|---|---|
| `3abe313` | Member cache policies and builder. |
| `c889665` | Presence cache flags. |
| `16acd2d` | Guild sticker cache. |

Next steps:
1. Re-evaluate the member policy on `PRESENCE_UPDATE`.
2. Add a voice state cache (PRD-API-028), which makes `VoicePolicy` work.
3. Record `CacheEvictions`.
4. Add an optional bounded message cache.

## 13. Decisions and rejected alternatives
- **Policy as a predicate over the wrapped member**: composable, testable with NSubstitute mocks of
  `IMember`, and independent from the event that triggered it.
- **Wire models in the store, wrappers on read**: the store stays smaller, and wrappers always see the
  latest presence and guild state.
- **Default `All`**: correct for small bots with no configuration. Large bots must opt down.
