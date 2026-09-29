# PRD-API-018 — Guilds

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-018](../specs/api-018-guilds.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/guild](https://docs.discord.com/developers/resources/guild) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
The guild resource is the largest in the API. It covers guild settings, channels (create and positions), members (list, search, add, modify, roles, kick), bans (single and bulk), roles (CRUD and positions), prune, regions, invites, integrations, widget, vanity URL, welcome screen, onboarding and incident actions. Bots need one coherent entry point per concern instead of a flat list of 45 methods.

## 2. Goals
- Every documented guild route is reachable from `IGuild` through per-resource facades (`Members`, `Bans`, `Roles`, `Channels`, `Prune`, `Widget`, `WelcomeScreen`, `Onboarding`, …).
- The guild, member and role objects are exposed completely.
- Endpoints Discord removed after bots lost guild ownership are flagged and obsoleted.

## 3. Out of scope
- Guild emojis, stickers, scheduled events, templates, soundboard, auto moderation, audit log, commands and webhooks have their own PRDs (017, 023, 019, 020, 024, 013, 012, 008, 026).
- Member, role and guild caching: [PRD-SDK-04](sdk-04-caching.md).

## 4. Usage scenarios
- As a bot author, I bulk-ban 200 raid accounts in one call.
- I give a member a role, and set a timeout with a reason.
- I set up onboarding prompts from a config file.
- I estimate a prune before running it.

## 5. Desired developer experience

```csharp
await guild.Bans.BulkBan(raiderIds, deleteMessageSeconds: 3600).WithReason("Raid").ExecuteAsync();
await guild.Members.AddRole(userId, verifiedRoleId).ExecuteAsync();
var count = await guild.Prune.Count(days: 30).ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| GD-F01 | Get Guild | `GET /guilds/{}` | Must | Partial | `GuildManager.GetAsync` (cache, then `GuildClient.GetAsync`) is reachable only through the concrete `DiscordClient.Guilds`; `IDiscordClient` has no guild lookup (guilds are reached through contexts and channels) |
| GD-F02 | Get Guild Preview | `GET /guilds/{}/preview` | Must | Implemented | `IGuild.GetPreview()` → `GuildClient.GetPreviewAsync` |
| GD-F03 | Modify Guild | `PATCH /guilds/{}` | Must | Implemented | `IGuild.Edit()` → `GuildClient.EditAsync` |
| GD-F04 | Get Guild Channels | `GET /guilds/{}/channels` | Must | Implemented | `GuildClient.GetChannelsAsync` ← `IGuildCategoryChannel.GetChannels()`; `IGuildChannels.GetAll()` reads the cache |
| GD-F05 | Create Guild Channel | `POST /guilds/{}/channels` | Must | Implemented | `IGuildChannels.Create()` → `GuildClient.CreateChannelAsync` |
| GD-F06 | Modify Guild Channel Positions | `PATCH /guilds/{}/channels` | Must | Implemented | `IGuildChannels.ModifyPositions()` → `GuildClient.ModifyChannelPositionsAsync` |
| GD-F07 | List Active Guild Threads | `GET /guilds/{}/threads/active` | Must | Implemented | `IGuildChannels.ListActiveThreads()` → `GuildClient.ListActiveThreadsAsync` |
| GD-F08 | Get Guild Member | `GET /guilds/{}/members/{}` | Must | Implemented | `IGuildMembers.Get()` (`MemberFetchMode.CacheThenRest`) → `MemberManager` → `GuildClient.GetMemberAsync` |
| GD-F09 | List Guild Members | `GET /guilds/{}/members` | Must | Implemented | `IGuildMembers.List()` → `GuildClient.GetMembersAsync` |
| GD-F10 | Search Guild Members | `GET /guilds/{}/members/search` | Must | Implemented | `IGuildMembers.Search()` → `GuildClient.SearchMembersAsync` |
| GD-F11 | Add Guild Member | `PUT /guilds/{}/members/{}` | Must | Implemented | `IGuildMembers.Add()` → `GuildClient.AddMemberAsync` |
| GD-F12 | Modify Guild Member | `PATCH /guilds/{}/members/{}` | Must | Implemented | `IGuildMembers.Modify()` → `GuildClient.ModifyMemberAsync` |
| GD-F13 | Modify Current Member | `PATCH /guilds/{}/members/@me` | Must | Implemented | `IGuildMembers.ModifyCurrent()` → `GuildClient.ModifyCurrentMemberAsync` |
| GD-F14 | Modify Current User Nick | `PATCH /guilds/{}/members/@me/nick` | Could | Deprecated | Deprecated by Discord in favour of Modify Current Member (GD-F13), which the SDK implements |
| GD-F15 | Add Guild Member Role | `PUT /guilds/{}/members/{}/roles/{}` | Must | Implemented | `IGuildMembers.AddRole()` → `GuildClient.AddMemberRoleAsync` |
| GD-F16 | Remove Guild Member Role | `DELETE /guilds/{}/members/{}/roles/{}` | Must | Implemented | `IGuildMembers.RemoveRole()` → `GuildClient.RemoveMemberRoleAsync` |
| GD-F17 | Remove Guild Member | `DELETE /guilds/{}/members/{}` | Must | Implemented | `IGuildMembers.Kick()` → `GuildClient.KickMemberAsync` |
| GD-F18 | Get Guild Bans | `GET /guilds/{}/bans` | Must | Implemented | `IGuildBans.List()` → `GuildClient.GetBansAsync` |
| GD-F19 | Get Guild Ban | `GET /guilds/{}/bans/{}` | Must | Implemented | `IGuildBans.Get()` → `GuildClient.GetBanAsync` |
| GD-F20 | Create Guild Ban | `PUT /guilds/{}/bans/{}` | Must | Implemented | `IGuildBans.Ban()` → `GuildClient.BanMemberAsync` |
| GD-F21 | Remove Guild Ban | `DELETE /guilds/{}/bans/{}` | Must | Implemented | `IGuildBans.Unban()` → `GuildClient.UnbanMemberAsync` |
| GD-F22 | Bulk Guild Ban | `POST /guilds/{}/bulk-ban` | Must | Implemented | `IGuildBans.BulkBan()` → `GuildClient.BulkBanAsync` |
| GD-F23 | Get Guild Roles | `GET /guilds/{}/roles` | Must | Implemented | `IGuildRoles.GetAll()` → `GuildClient.GetRolesAsync` |
| GD-F24 | Get Guild Role | `GET /guilds/{}/roles/{}` | Must | Implemented | `IGuildRoles.Get()` → `RoleClient.GetAsync` |
| GD-F25 | Get Guild Role Member Counts | `GET /guilds/{}/roles/member-counts` | Should | Missing | — |
| GD-F26 | Create Guild Role | `POST /guilds/{}/roles` | Must | Implemented | `IGuildRoles.Create()`, `IRole.CreateCopy()` → `RoleClient.CreateAsync` |
| GD-F27 | Modify Guild Role Positions | `PATCH /guilds/{}/roles` | Must | Implemented | `IGuildRoles.ModifyPositions()`, `IRole.ModifyPosition()` → `RoleClient.ModifyPositionsAsync` |
| GD-F28 | Modify Guild Role | `PATCH /guilds/{}/roles/{}` | Must | Implemented | `IRole.Edit()` → `RoleClient.EditAsync` |
| GD-F29 | Delete Guild Role | `DELETE /guilds/{}/roles/{}` | Must | Implemented | `IRole.Delete()` → `RoleClient.DeleteAsync` |
| GD-F30 | Get Guild Prune Count | `GET /guilds/{}/prune` | Must | Implemented | `IGuildPrune.Count()` → `GuildClient.GetPruneCountAsync` |
| GD-F31 | Begin Guild Prune | `POST /guilds/{}/prune` | Must | Implemented | `IGuildPrune.Begin()` → `GuildClient.BeginPruneAsync` |
| GD-F32 | Get Guild Voice Regions | `GET /guilds/{}/regions` | Must | Implemented | `IGuild.GetVoiceRegions()` → `GuildClient.GetVoiceRegionsAsync` |
| GD-F33 | Get Guild Invites | `GET /guilds/{}/invites` | Must | Implemented | `IGuild.GetInvites()` → `GuildClient.GetInvitesAsync` |
| GD-F34 | Get Guild Integrations | `GET /guilds/{}/integrations` | Must | Implemented | `IGuild.GetIntegrations()` → `GuildClient.ListIntegrationsAsync` |
| GD-F35 | Delete Guild Integration | `DELETE /guilds/{}/integrations/{}` | Must | Implemented | `IIntegration.Delete()` → `GuildClient.DeleteIntegrationAsync` |
| GD-F36 | Get Guild Widget Settings | `GET /guilds/{}/widget` | Should | Missing | Only the public widget JSON (GD-F38) and edit (GD-F37) are implemented |
| GD-F37 | Modify Guild Widget | `PATCH /guilds/{}/widget` | Must | Implemented | `IGuildWidgetSurface.Edit()` → `GuildClient.EditWidgetAsync` |
| GD-F38 | Get Guild Widget | `GET /guilds/{}/widget.json` | Must | Implemented | `IGuildWidgetSurface.Get()` → `GuildClient.GetWidgetAsync` |
| GD-F39 | Get Guild Vanity URL | `GET /guilds/{}/vanity-url` | Must | Implemented | `IGuild.GetVanityUrl()` → `GuildClient.GetVanityUrlAsync` |
| GD-F40 | Get Guild Widget Image | `GET /guilds/{}/widget.png` | Could | Missing | — |
| GD-F41 | Get Guild Welcome Screen | `GET /guilds/{}/welcome-screen` | Must | Implemented | `IGuildWelcomeScreen.Get()` → `GuildClient.GetWelcomeScreenAsync` |
| GD-F42 | Modify Guild Welcome Screen | `PATCH /guilds/{}/welcome-screen` | Must | Implemented | `IGuildWelcomeScreen.Edit()` → `GuildClient.EditWelcomeScreenAsync` |
| GD-F43 | Get Guild Onboarding | `GET /guilds/{}/onboarding` | Must | Implemented | `IGuildOnboardingSurface.Get()` → `GuildTemplateClient.GetOnboardingAsync` |
| GD-F44 | Modify Guild Onboarding | `PUT /guilds/{}/onboarding` | Must | Implemented | `IGuildOnboarding.Modify()`, `IGuildOnboardingSurface.Edit()` → `GuildTemplateClient.ModifyOnboardingAsync` |
| GD-F45 | Modify Guild Incident Actions | `PUT /guilds/{}/incident-actions` | Must | Implemented | `IGuild.ModifyIncidentActions()` → `GuildClient.ModifyIncidentActionsAsync` |
| GD-F46 | Guild object (settings, features, premium tier, verification, notification and content-filter levels, system channel flags, NSFW level, locale) | — | Must | Implemented | `IGuild`, `VerificationLevel`, `DefaultMessageNotificationLevel`, `ExplicitContentFilterLevel`, `PremiumTier`, `SystemChannelFlags`, `MfaLevel` |
| GD-F47 | Guild member object (nick, avatar, banner, roles, joined, boosting, pending, timeout, flags) | — | Must | Implemented | `IMember`, `GuildMemberFlags` |
| GD-F48 | Member timeouts (`communication_disabled_until`) | — | Must | Implemented | `IMember.TimeoutForAsync`, `IMember.TimeoutUntilAsync`, `IMember.RemoveTimeoutAsync` |
| GD-F49 | Onboarding object, prompts and prompt options | — | Should | Implemented | `IGuildOnboarding`, `OnboardingPromptBuilder`, `OnboardingMode`, `OnboardingPromptType` |
| GD-F50 | Welcome screen and widget objects | — | Should | Implemented | `IGuildWelcomeScreen`, `IGuildWidgetSurface` |
| GD-F51 | Unavailable guilds (outages) | — | Must | Implemented | `GuildManager` (PRD-API-003, GW-F21) |
| GD-F52 | Delete Guild (bots can no longer own guilds) | `DELETE /guilds/{}` | Could | Deprecated | No longer in Discord's docs at the baseline (guild ownership by apps was removed, change log 2025-04-15); still exposed as `IGuild.Delete()` → `GuildClient.DeleteAsync` |
| GD-F53 | Modify Guild MFA Level (owner-only) | `POST /guilds/{}/mfa` | Could | Deprecated | No longer in Discord's docs at the baseline; still exposed as `IGuild.ModifyMfaLevel()` → `GuildClient.ModifyMfaLevelAsync` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| GD-N01 | Every mutating guild operation accepts `WithReason`. |
| GD-N02 | Member-list operations check the `GUILD_MEMBERS` intent up front (`IntentGuard`), because Discord returns partial data without it. |

## 8. Configuration
None.

## 9. Compatibility
`IGuild.Delete()` and `IGuild.ModifyMfaLevel()` target endpoints Discord removed. Mark them `[Obsolete]` for one minor release, then delete them. The per-resource facade split (commit `54a5c3e`) was a breaking change that already shipped.

## 10. Acceptance criteria
- [x] Guild routes (`GuildClientTests`, `RoleClientTests`, `AddMemberActionTests`, `ModifyMemberActionTests`, `ModifyRolePositionsActionTests`, `IntentGuardedActionsTests`).
- [x] Wrappers (`GuildWrapperTests`, `GuildMemberWrapperTests`, `RoleWrapperTests`, `GuildOnboardingWrapperTests`, `OnboardingPromptBuilderTests`, `IntegrationWrapperTests`).
- [ ] `IDiscordClient` exposes guild lookup (GD-F01).
- [ ] Role member counts, widget settings and image, current-user nick (GD-F25, GD-F36, GD-F40, GD-F14).
- [ ] `IGuild.Delete()` and `IGuild.ModifyMfaLevel()` are obsoleted (GD-F52, GD-F53).

## 11. Open questions
- None.
