# SPEC-API-006 — Permissions and roles model

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-006](../prd/api-006-permissions.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [topics/permissions](https://docs.discord.com/developers/topics/permissions) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`DiscordPermission` (a `ulong` flags enum) models the bits. `ChannelPermissionCalculator` computes a
member's effective permissions in a channel from cached roles and the channel's `PermissionOverwrite[]`
(through `IPermissionContainer`). The role object is exposed as `IRole`, implemented by `RoleWrapper`.

## 2. Projects and dependencies
- `DiscoSdk`: `DiscordPermission`, `PermissionOverwrite`, `PermissionOverwriteType`, `IRole`, `IRoleTags`,
  `RoleColors`, `RoleFlags`, `IPermissionContainer`, `IPermissionHolder`, `DiscordPermissionConverter`.
- `DiscoSdk.Hosting`: `ChannelPermissionCalculator`, `ChannelPermissionContainer`, `RoleWrapper`, `Role`.

## 3. Models and contracts
- `Role` (wire model): `id`, `name`, `colors`, `hoist`, `icon`, `unicode_emoji`, `position`,
  `permissions`, `managed`, `mentionable`, `tags`, `flags`.
- `IRole` exposes everything except `flags`. `Colors` is a single `Color` (see PM-F08).

## 4. Components
| Type | Responsibility |
|---|---|
| `ChannelPermissionCalculator(IGuild, IPermissionContainer)` | `GetPermission(IMember)`: owner check, base permissions, admin short-circuit, role overwrites, member overwrite. |
| `ChannelPermissionContainer` | Adapts a channel model's overwrites; `UpsertPermissionOverride` / `DeletePermissionOverride` REST (PRD-API-014). |
| `DiscordPermissionConverter` | JSON string ↔ `ulong`. |

## 5. Public API
- `IGuildChannelBase.GetPermission(IMember)`, `IPermissionContainer.PermissionOverwrites`.
- `IRole.Permissions`, `IMember.Roles`, `IRole.CompareTo` (hierarchy).

## 6. Discord surface
There are 52 permission bits (0–52 at the baseline; bit 47 is not assigned). Overwrite types are 0 (role) and 1 (member).

## 7. Flows
**Discord's algorithm** (target):
1. If the member is the owner, grant all.
2. Base = @everyone role ∪ member role permissions. If `ADMINISTRATOR`, grant all.
3. Apply the @everyone overwrite (deny, then allow).
4. OR together the denies of all member-role overwrites, and separately the allows; apply the deny, then the allow.
5. Apply the member overwrite (deny, then allow).
6. Implicit rules: without `VIEW_CHANNEL`, grant nothing; without `SEND_MESSAGES`, strip the send-dependent bits.
7. If the member is timed out, keep only `VIEW_CHANNEL` and `READ_MESSAGE_HISTORY`.

**Current implementation:** steps 1–2 match. Instead of steps 3–4, role overwrites are applied one at a
time by descending role position (allow, then deny). The @everyone overwrite is only applied if the member's
role list contains @everyone. Step 5 applies allow before deny. Steps 6–7 are missing.

## 8. Concurrency and lifecycle
The calculator is stateless per call and reads immutable snapshots of cached roles.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Role A denies and role B allows `SEND_MESSAGES` in a channel | Allowed (currently depends on role positions). |
| @everyone overwrite denies `VIEW_CHANNEL` | Member cannot view unless a role or member overwrite allows it (currently ignored). |
| Thread channel | Uses the parent's overwrites (currently uses the thread's, which are empty). |
| Roles not cached (no `Guilds` intent) | Returns base permissions from what is cached; `IntentGuard` warns. |

## 10. Observability
None specific.

## 11. Tests
| Test class | Covers |
|---|---|
| `ChannelPermissionContainerTests` | PM-F01, PM-F02 (partial) |
| `OverridePermissionActionTests` | PM-F02 (overwrite REST) |
| `DiscordPermissionConverterTests` | PM-F11, PM-N02 |
| `RoleWrapperTests` | PM-F07 – PM-F09 |

Implemented or Partial requirements without a covering test: PM-F05 – PM-F06, PM-F10, PM-F12 – PM-F63, PM-N01.

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Permission overwrite edit/delete. |
| `9f81c56` | Interface-first `IRole`. |

Next steps:
1. Rewrite `ChannelPermissionCalculator` to Discord's algorithm and add tests for Discord's documented examples.
2. Resolve threads through the parent container.
3. Add bits 48–52 to `DiscordPermission`.
4. Add `IRole.Flags` and `RoleColors` on `IRole`.

## 13. Decisions and rejected alternatives
- **Cache-only computation**: permission checks must be cheap and synchronous, so REST fallback is left to the caller.
