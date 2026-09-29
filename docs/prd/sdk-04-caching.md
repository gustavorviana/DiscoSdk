# PRD-SDK-04 — Caching and managers

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-SDK-04](../specs/sdk-04-caching.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | — (SDK framework feature; gateway sources in PRD-API-004) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Bots read guilds, channels, members, presences and stickers constantly. REST calls for each read are slow
and rate-limited. Caching everything, on the other hand, costs memory on large guilds, and privileged
intents decide what data arrives at all. Bot authors need caches fed by gateway events, with explicit
per-entity policies, cache/REST fallback modes, and diagnostics when a misconfiguration leaves a cache
empty.

## 2. Goals
- Gateway-fed caches for guilds, channels, users, members, presences and guild stickers.
- A configurable member cache policy (presets, a composable builder and custom predicates).
- Presence and sticker caches that are opt-in at field or entity level.
- Lookups with an explicit fetch mode (`CacheOnly`, `CacheThenRest`, `RestOnly`) and write-back.
- Warnings when the cache is read without the intent that feeds it.

## 3. Out of scope
- The events themselves: [PRD-API-004](api-004-gateway-events.md). Request Guild Members (op 8):
  [PRD-API-003](api-003-gateway-connection.md). Member REST routes: [PRD-API-018](api-018-guilds.md).
- Distributed caches (Redis) across processes.

## 4. Usage scenarios
- As a bot author on a 200k-member guild, I cache only the owner and members with a staff role.
- A dashboard bot reads `IMember.OnlineStatus` from the presence cache without storing activities.
- A moderation bot reads a member from the cache and falls back to REST on a miss.
- The bot forgets the `Guilds` intent, and the SDK warns the first time the guild cache is read.

## 5. Desired developer experience

```csharp
var client = DiscordClientBuilder.Create(token)
    .WithIntents(DiscordIntent.Guilds | DiscordIntent.GuildMembers | DiscordIntent.GuildPresences)
    .WithMemberCachePolicy(PolicyMode.Any, p => p
        .IncludeOwner()
        .IncludeRoles(staffRoleId))
    .WithPresenceCache(PresenceCacheFlag.ClientStatus)
    .WithStickerCache(StickerCacheFlag.Guild)
    .Build();

var member = await client.Members.Get(guildId, userId, MemberFetchMode.CacheThenRest).ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| CA-F01 | Guild cache fed by `GUILD_CREATE` / `GUILD_UPDATE` / `GUILD_DELETE`, with pending-guild tracking until fully initialised | — | Must | Implemented | `GuildManager.HandleGuildCreate`, `GuildManager.IsFullyInitialized`, `GuildManager.PendingGuildsCount` |
| CA-F02 | Channel cache with REST fallback | — | Must | Implemented | `ChannelManager.Get` (cache, then `GET /channels/{}` with write-back) |
| CA-F03 | User cache with REST fallback | — | Should | Implemented | `UserRepository.Get`, `UserRepository.Upsert` |
| CA-F04 | Member cache fed by `GUILD_CREATE`, member add/update/remove and chunks | — | Must | Implemented | `MemberManager.OnGuildMembersSeed`, `MemberManager.OnMemberAddOrUpdateAsync`, `MemberManager.OnMembersChunkAsync` |
| CA-F05 | Member cache policy presets (None, Owner, Voice, Online, All; default All) | — | Must | Partial | `MemberCachePolicy`, `MemberCachePolicyExtensions.ToPolicy`. `OnlinePolicy` caches Online, Idle and DoNotDisturb members. `VoicePolicy` never matches because `IMember.VoiceState` is always null (PRD-API-028). |
| CA-F06 | Composable policy builder (all/any, owner, voice, online, boosters, pending, roles, predicate, nesting) | — | Must | Implemented | `MemberCachePolicyBuilder`, `PolicyMode` |
| CA-F07 | Custom policy by instance or dependency-injection factory | — | Should | Implemented | `IMemberCachePolicy`, `DiscordClientBuilder.WithMemberCachePolicy` |
| CA-F08 | Re-evaluate the member policy when presence or voice state changes | — | Should | Missing | `PRESENCE_UPDATE` only updates `PresenceManager`; no voice state cache exists, so Online/Voice decisions only change on member events |
| CA-F09 | Member lookup with fetch mode and policy-gated write-back | — | Must | Implemented | `IMemberManager.Get`, `MemberFetchMode`, `IGuildMembers.GetCached`, `IGuildMembers.GetCachedCount` |
| CA-F10 | Presence cache with field flags (client status, activities; default client status) | — | Should | Implemented | `PresenceManager`, `PresenceCacheFlag`, `DiscordClientBuilder.WithPresenceCache` |
| CA-F11 | Guild sticker cache (opt-in) with fetch mode | — | Could | Implemented | `StickerManager`, `StickerCacheFlag`, `StickerFetchMode`, `DiscordClientBuilder.WithStickerCache` |
| CA-F12 | Eviction on guild removal | — | Must | Implemented | `MemberManager.OnGuildRemoveAsync`, `PresenceManager.OnGuildRemove`, `StickerManager.OnGuildRemove` |
| CA-F13 | Missing-intent warning on cache reads | — | Should | Implemented | `GuildCacheWarnTracker.WarnMissing` (logs once per process) |
| CA-F14 | Cache lookup metrics (hit, miss, rest) | — | Should | Implemented | `DiscoSdkDiagnostics.CacheLookups` (member and sticker managers) |
| CA-F15 | Cache eviction metrics | — | Could | Partial | `DiscoSdkDiagnostics.CacheEvictions` is declared but never recorded |
| CA-F16 | Message cache (recent messages per channel) for `MESSAGE_UPDATE` / `MESSAGE_DELETE` before-state | — | Could | Missing | — |
| CA-F17 | Size limits or expiry for caches | — | Could | Missing | All caches are unbounded `ConcurrentDictionary` maps |
| CA-F18 | Voice state cache | — | Should | Missing | `IGuildVoiceState` is empty (PRD-API-028) |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| CA-N01 | Cache reads are synchronous and lock-free (`ConcurrentDictionary`), safe across shard workers. |
| CA-N02 | Policies are pure, thread-safe predicates with no I/O. |
| CA-N03 | Memory is proportional to the policy: `MemberCachePolicy.None` keeps no members. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Member cache policy | `All` | preset, builder, instance, factory | `WithMemberCachePolicy` |
| Presence cache fields | `ClientStatus` | `PresenceCacheFlag` | `WithPresenceCache` |
| Guild sticker cache | `None` | `StickerCacheFlag` | `WithStickerCache` |

## 9. Compatibility
`OnlinePolicy` used to cache only `Invisible` members (an operator precedence bug), which never matched
because Discord reports invisible users as offline. It now caches Online, Idle and DoNotDisturb members.

## 10. Acceptance criteria
- [x] Policy presets and builder (`PolicyPresetsTests`, `MemberCachePolicyBuilderTests`).
- [x] Member seeding and dispatch (`MemberManagerSeedTests`, `MemberCacheDispatchTests`).
- [x] Presence flags (`PresenceManagerTests`); sticker cache (`StickerCacheDispatchTests`).
- [x] Lookup metrics (`CacheLookupsMetricTests`); intent guard (`GuildCacheIntentGuardTests`).
- [x] `OnlinePolicy` caches Online, Idle and DoNotDisturb members (CA-F05; `PolicyPresetsTests`).
- [ ] Presence changes re-evaluate the member policy (CA-F08).

## 11. Open questions
- Should `CA-F08` upsert from the presence payload alone (it carries only a partial user), or trigger a
  REST fetch? Provisional: evict on offline; on online, upsert only when the member is already known from
  `GUILD_CREATE` or chunks.
