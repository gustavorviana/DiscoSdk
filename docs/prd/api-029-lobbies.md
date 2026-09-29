# PRD-API-029 — Lobbies (Social SDK server side)

| | |
|---|---|
| **Status** | Draft |
| **Spec** | [SPEC-API-029](../specs/api-029-lobbies.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/lobby](https://docs.discord.com/developers/resources/lobby) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Lobbies are Social SDK objects for game matchmaking: groups of players with metadata, messaging, and an optional link to a guild channel. A game backend creates lobbies, manages members, links channels and moderates lobby messages with the bot token. A .NET game backend that already uses DiscoSdk for its bot has no API for them.

## 2. Goals
- Map the 15 lobby routes (16 documented operations, since link and unlink share one route) with typed lobby, member and message models.
- Keep the surface separate from the bot-centric API, because it only matters to games using the Social SDK.

## 3. Out of scope
- The Social SDK client (C++/Unity/Unreal): excluded, see `docs/README.md`.
- Lobby webhook events: [PRD-API-005](api-005-webhook-events.md).
- Identity profiles: [PRD-API-011](api-011-application.md).

## 4. Usage scenarios
- As a game-backend author, when a match starts I create a lobby with the players and link it to the guild's match channel.
- I remove a cheater from the lobby and flag their message.

## 5. Desired developer experience

```csharp
// (proposed)
var lobby = await client.Lobbies.Create()
    .AddMember(playerId, metadata: new() { ["team"] = "red" })
    .ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| LB-F01 | Create Lobby | `POST /lobbies` | Could | Missing | Social SDK roadmap |
| LB-F02 | Create or Join Lobby | `PUT /lobbies` | Could | Missing | Social SDK roadmap |
| LB-F03 | Get Lobby | `GET /lobbies/{}` | Could | Missing | Social SDK roadmap |
| LB-F04 | Modify Lobby | `PATCH /lobbies/{}` | Could | Missing | Social SDK roadmap |
| LB-F05 | Delete Lobby | `DELETE /lobbies/{}` | Could | Missing | Social SDK roadmap |
| LB-F06 | Add a Member to a Lobby | `PUT /lobbies/{}/members/{}` | Could | Missing | Social SDK roadmap |
| LB-F07 | Bulk Update Lobby Members | `POST /lobbies/{}/members/bulk` | Could | Missing | Social SDK roadmap |
| LB-F08 | Remove a Member from a Lobby | `DELETE /lobbies/{}/members/{}` | Could | Missing | Social SDK roadmap |
| LB-F09 | Leave Lobby | `DELETE /lobbies/{}/members/@me` | Could | Missing | Social SDK roadmap |
| LB-F10 | Link Channel to Lobby / Unlink Channel from Lobby | `PATCH /lobbies/{}/channel-linking` | Could | Missing | Social SDK roadmap |
| LB-F11 | Send Lobby Message | `POST /lobbies/{}/messages` | Could | Missing | Social SDK roadmap |
| LB-F12 | Get Lobby Messages | `GET /lobbies/{}/messages` | Could | Missing | Social SDK roadmap |
| LB-F13 | Update Lobby Message Moderation Metadata | `PUT /lobbies/{}/messages/{}/moderation-metadata` | Could | Missing | Social SDK roadmap |
| LB-F14 | Create Lobby Channel Invite for Self | `POST /lobbies/{}/members/@me/invites` | Could | Missing | Social SDK roadmap |
| LB-F15 | Create Lobby Channel Invite for User | `POST /lobbies/{}/members/{}/invites` | Could | Missing | Social SDK roadmap |
| LB-F16 | Lobby, lobby member and lobby message objects (metadata, flags, linked channel) | — | Could | Missing | — |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| LB-N01 | The lobby surface is opt-in, for example `IDiscordClient.Lobbies` in a `DiscoSdk.Social` package (proposed), so bot-only users do not see it. |

## 8. Configuration
None.

## 9. Compatibility
New, opt-in surface; additive.

## 10. Acceptance criteria
- [ ] All lobby routes are reachable through a typed surface (LB-F01 – LB-F15).
- [ ] Lobby webhook events are routed when PRD-API-005 lands.

## 11. Open questions
- Separate package (`DiscoSdk.Social`) or part of the core? Provisional: a separate package together with identity profiles (PRD-API-011) and the Social SDK webhook events.
