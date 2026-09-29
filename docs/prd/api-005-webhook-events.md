# PRD-API-005 — Webhook events (HTTP)

| | |
|---|---|
| **Status** | Draft |
| **Spec** | [SPEC-API-005](../specs/api-005-webhook-events.md) |
| **Projects** | `DiscoSdk`, new `DiscoSdk.AspNetCore` (proposed) |
| **Discord docs** | [events/webhook-events](https://docs.discord.com/developers/events/webhook-events) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Some events are only delivered over HTTP to the app's *Webhook Events URL*, never over the gateway:
`APPLICATION_AUTHORIZED` and `APPLICATION_DEAUTHORIZED` (user installs), quest enrollment, and the Social SDK
lobby and game-DM events. Entitlement events also arrive this way for apps without a gateway connection.
Today a DiscoSdk bot cannot receive any of them. Bot authors would have to hand-roll Ed25519 verification
and payload parsing.

## 2. Goals
- An ASP.NET Core endpoint that verifies `X-Signature-Ed25519` and `X-Signature-Timestamp`, answers
  `PING` with 204 and acknowledges events within 3 s.
- Events are routed to typed handlers that reuse the gateway handler model (`IDiscordEventHandler<TContext>`).
- The core `DiscoSdk.Hosting` package does not depend on ASP.NET Core.

## 3. Out of scope
- The HTTP *interactions* endpoint (the same signature scheme) is covered in [PRD-API-009](api-009-interactions.md).
  The verification code should be shared.
- Configuring the URL in the developer portal (manual step).

## 4. Usage scenarios
- As a bot author, when a user installs my app to their account, I receive `APPLICATION_AUTHORIZED` with
  the user and scopes, and I record the install.
- When a request fails signature validation, the endpoint returns 401 without touching my handlers.

## 5. Desired developer experience

```csharp
// (proposed) DiscoSdk.AspNetCore
builder.Services.AddDiscoWebhookEvents(o => o.PublicKey = config["Discord:PublicKey"])
    .AddHandler<InstallTracker>();

app.MapDiscoWebhookEvents("/discord/events");

public sealed class InstallTracker : IApplicationAuthorizedHandler   // (proposed)
{
    public Task HandleAsync(IApplicationAuthorizedContext context) => /* … */ Task.CompletedTask;
}
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| WE-F01 | Webhook Events URL endpoint (`POST`, JSON) | — | Should | Missing | — |
| WE-F02 | Acknowledge `PING` (`type: 0`) with 204 and an empty body | — | Should | Missing | — |
| WE-F03 | Validate `X-Signature-Ed25519` / `X-Signature-Timestamp`, 401 on failure | — | Must | Missing | — |
| WE-F04 | Acknowledge events with 204 within 3 seconds; process asynchronously | — | Should | Missing | — |
| WE-F05 | Outer payload (`version`, `application_id`, `type`, `event`) and event body (`type`, `timestamp`, `data`) | — | Should | Missing | — |
| WE-F06 | Read the configured URL from the application object | — | Could | Implemented | `IApplication.EventWebhooksUrl` |
| WE-F07 | Webhook event `APPLICATION_AUTHORIZED` | `webhook-event:APPLICATION_AUTHORIZED` | Should | Missing | — |
| WE-F08 | Webhook event `APPLICATION_DEAUTHORIZED` | `webhook-event:APPLICATION_DEAUTHORIZED` | Should | Missing | — |
| WE-F09 | Webhook event `ENTITLEMENT_CREATE` | `webhook-event:ENTITLEMENT_CREATE` | Should | Missing | Same data is dispatched over the gateway (`IEntitlementCreateHandler` family, PRD-API-027); no HTTP receiver |
| WE-F10 | Webhook event `ENTITLEMENT_UPDATE` | `webhook-event:ENTITLEMENT_UPDATE` | Should | Missing | Same data is dispatched over the gateway (`IEntitlementCreateHandler` family, PRD-API-027); no HTTP receiver |
| WE-F11 | Webhook event `ENTITLEMENT_DELETE` | `webhook-event:ENTITLEMENT_DELETE` | Should | Missing | Same data is dispatched over the gateway (`IEntitlementCreateHandler` family, PRD-API-027); no HTTP receiver |
| WE-F12 | Webhook event `QUEST_USER_ENROLLMENT` | `webhook-event:QUEST_USER_ENROLLMENT` | Could | Missing | — |
| WE-F13 | Webhook event `LOBBY_MESSAGE_CREATE` | `webhook-event:LOBBY_MESSAGE_CREATE` | Could | Missing | Social SDK (PRD-API-029) |
| WE-F14 | Webhook event `LOBBY_MESSAGE_UPDATE` | `webhook-event:LOBBY_MESSAGE_UPDATE` | Could | Missing | Social SDK (PRD-API-029) |
| WE-F15 | Webhook event `LOBBY_MESSAGE_DELETE` | `webhook-event:LOBBY_MESSAGE_DELETE` | Could | Missing | Social SDK (PRD-API-029) |
| WE-F16 | Webhook event `GAME_DIRECT_MESSAGE_CREATE` | `webhook-event:GAME_DIRECT_MESSAGE_CREATE` | Could | Missing | Social SDK (PRD-API-029) |
| WE-F17 | Webhook event `GAME_DIRECT_MESSAGE_UPDATE` | `webhook-event:GAME_DIRECT_MESSAGE_UPDATE` | Could | Missing | Social SDK (PRD-API-029) |
| WE-F18 | Webhook event `GAME_DIRECT_MESSAGE_DELETE` | `webhook-event:GAME_DIRECT_MESSAGE_DELETE` | Could | Missing | Social SDK (PRD-API-029) |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| WE-N01 | Signature verification in constant time, on the raw request body (before JSON parsing). |
| WE-N02 | No ASP.NET Core dependency in `DiscoSdk` / `DiscoSdk.Hosting`; the endpoint ships as a separate package. |
| WE-N03 | Handlers run after the 204 is sent, so a slow handler never causes Discord to disable the endpoint. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Public key | required | 64 hex chars | (proposed) `DiscoWebhookEventOptions.PublicKey` |
| Route | `/discord/events` | any path | (proposed) `MapDiscoWebhookEvents(pattern)` |

## 9. Compatibility
Purely additive. Entitlement handlers (`IEntitlementCreateHandler`, …) should be reused, so a bot
receives the same context over the gateway or over HTTP.

## 10. Acceptance criteria
- [ ] Invalid signatures get 401; valid `PING`s get 204 (WE-F02, WE-F03).
- [ ] Each event type dispatches to its handler with a typed context (WE-F07–F18).
- [ ] A handler taking 10 s does not delay the 204 (WE-N03).

## 11. Open questions
- Ed25519 implementation: .NET has no built-in Ed25519 before .NET 10. Provisional: use
  `NSec.Cryptography` in the ASP.NET Core package, or target .NET 10 for that package only.
- Social SDK events (lobby messages, game DMs): same package or a later `DiscoSdk.Social`? Provisional:
  same package, but handler interfaces added only with PRD-API-029.
