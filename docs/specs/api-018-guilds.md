# SPEC-API-018 — Guilds

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-018](../prd/api-018-guilds.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/guild](https://docs.discord.com/developers/resources/guild) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`GuildWrapper` (`IGuild`) holds guild data and delegates each concern to a facade:
- `Members` (`IGuildMembers`, backed by `MemberManager`);
- `Bans` (`GuildBansSurface`), `Roles` (`GuildRolesSurface`), `Channels` (`GuildChannelsSurface`), `Prune` (`GuildPruneSurface`);
- `Widget` (`GuildWidgetSurfaceImpl`), `WelcomeScreen` (`GuildWelcomeScreenSurface`), `Onboarding` (`GuildOnboardingSurfaceImpl`);
- and the facades owned by other PRDs (emojis, stickers, scheduled events, templates, soundboard, auto moderation, commands).

REST goes through `GuildClient`, `RoleClient` and `GuildTemplateClient` (onboarding). Reads prefer the caches (`GuildManager`, `MemberManager`).

## 2. Projects and dependencies
`DiscoSdk` (`IGuild`, `IGuildMembers`, `IGuildBans`, `IGuildRoles`, `IGuildChannels`, `IGuildPrune`, `IGuildWidgetSurface`, `IGuildWelcomeScreen`, `IGuildOnboarding`, `IMember`, `IRole`, the builder actions, and the guild enums), and `DiscoSdk.Hosting` (`GuildClient`, `RoleClient`, `Surfaces/*`, `GuildWrapper`, `GuildMemberWrapper`, `RoleWrapper`, `Managers/*`).

## 3. Models and contracts
The wire `Guild` maps the documented fields, including features, premium progress bar and safety alerts channel. The wire `GuildMember` maps nick, avatar, banner, roles, joined, premium since, deaf/mute, flags, pending, permissions and `communication_disabled_until`.

## 4. Components
| Type | Responsibility |
|---|---|
| `GuildClient` | Guild get/edit, preview, channels, members (list, search, add, modify, modify current, roles, kick), bans (single, bulk), prune, regions, invites, integrations, widget, vanity, welcome screen, incident actions, active threads, and the removed delete/MFA routes. |
| `RoleClient` | Role get/create/edit/delete/positions. |
| Surfaces | Bind the guild id and return typed actions. The bulk-ban size (1–200) is documented, but Discord enforces it (400), not the SDK. |
| `MemberManager`, `GuildManager` | Cache-first reads with REST fallback (SPEC-SDK-04). |

## 5. Public API
`IGuild.Edit()`, `GetPreview()`, `GetAuditLogs()`, `GetInvites()`, `GetIntegrations()`, `GetVoiceRegions()`, `GetVanityUrl()`, `ModifyIncidentActions()`, `Leave()`, and the facades `Members`, `Bans`, `Roles`, `Channels`, `Prune`, `Widget`, `WelcomeScreen`, `Onboarding`. There is no guild lookup on `IDiscordClient` (GD-F01).

## 6. Discord surface
45 documented routes, plus 2 removed routes still called (`DELETE /guilds/{id}`, `POST /guilds/{id}/mfa`). The member list needs the privileged `GUILD_MEMBERS` intent.

## 7. Flows
Bulk ban: `POST /guilds/{id}/bulk-ban { user_ids, delete_message_seconds }` → the banned ids are returned (more than 200 ids → Discord 400).

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Member list or member request without the `GUILD_MEMBERS` intent | `MissingIntentException` from `IntentGuard.Require` (`MemberPaginationAction`, `RequestGuildMembersAction`), before any request. |
| Role above the bot's highest role | Discord 403 → `InsufficientPermissionException`. |
| `IGuild.Delete()` / `ModifyMfaLevel()` | Fail: the endpoints are not available to applications. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `GuildClientTests` | GD-F01 – GD-F45 (client level) |
| `RoleClientTests`, `ModifyRolePositionsActionTests`, `RoleWrapperTests` | GD-F23 – GD-F29 |
| `AddMemberActionTests`, `ModifyMemberActionTests`, `GuildMemberWrapperTests`, `IntentGuardedActionsTests` | GD-F08 – GD-F17, GD-F47, GD-F48, GD-N02 |
| `GuildWrapperTests` | GD-F46 |
| `GuildOnboardingWrapperTests`, `OnboardingPromptBuilderTests` | GD-F43, GD-F44, GD-F49 |
| `IntegrationWrapperTests` | GD-F34, GD-F35 |

## 12. History
| Commit | Change |
|---|---|
| `82cbb28` | `GetRole`, `ModifyRolePositions` builder, `ListActiveThreads`. |
| `ec7f1ef` | Member API moved to `IGuildMembers`. |
| `cfd6a43` | Member and ban mutations split into facades. |
| `54a5c3e` | `IGuild` split into per-resource facades. |
| `9c1343a` | `IntentGuard` pre-checks on member-list operations. |

Next steps:
1. Add `IDiscordClient.GetGuild(id)` (cache, then REST).
2. Add role member counts, widget settings and the widget image.
3. Obsolete `IGuild.Delete()` and `IGuild.ModifyMfaLevel()`.

## 13. Decisions and rejected alternatives
- **Per-resource facades** (a breaking change in `54a5c3e`) rather than 45 methods on `IGuild`: this gives discoverability, keeps the interface small, and lets each facade own its cache policy.
