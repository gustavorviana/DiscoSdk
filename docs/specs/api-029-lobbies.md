# SPEC-API-029 — Lobbies (Social SDK server side)

| | |
|---|---|
| **Status** | Draft |
| **PRD** | [PRD-API-029](../prd/api-029-lobbies.md) |
| **Projects** | `DiscoSdk.Social` (proposed) |
| **Discord docs** | [resources/lobby](https://docs.discord.com/developers/resources/lobby) |
| **Last updated** | 2026-09-29 |

## 1. Summary
Proposed: a `LobbyClient` in a new `DiscoSdk.Social` package that implements the 15 lobby routes, exposed as `ILobbies` (`Create`, `CreateOrJoin`, `Get`, `Modify`, `Delete`, members, link/unlink channel, messages, moderation metadata, invites). It reuses `DiscordRestClient` from the host client.

## 2. Projects and dependencies
`DiscoSdk.Social` (proposed) → `DiscoSdk.Hosting`. No native dependencies.

## 3. Models and contracts
`Lobby { Id, ApplicationId, Metadata, Members, LinkedChannel }`, `LobbyMember { Id, Metadata, Flags }` and `LobbyMessage` (proposed).

## 4. Components
| Type | Responsibility |
|---|---|
| `LobbyClient` (proposed) | All lobby routes. |
| `LobbiesSurface` (proposed) | Fluent builders for create and modify. |

## 5. Public API
(proposed) `IDiscordClient.Lobbies()` extension from `DiscoSdk.Social`.

## 6. Discord surface
15 routes under `/lobbies` (link and unlink share `PATCH /lobbies/{id}/channel-linking`).

## 7. Flows
Create: `POST /lobbies { metadata, members[] }` → `Lobby`.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Metadata over 1000 characters | Validated locally, then Discord 400. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `LobbyClientTests` (proposed) | LB-F01 – LB-F16 |

## 12. Implementation plan
| Commit | Change |
|---|---|
| `—` | Not started. |

Steps: 1. models and `LobbyClient`; 2. surface and builders; 3. Social SDK webhook events (PRD-API-005).

## 13. Decisions and rejected alternatives
- **Separate package**: lobbies target game backends, not typical bots.
