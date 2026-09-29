# SPEC-API-005 — Webhook events (HTTP)

| | |
|---|---|
| **Status** | Draft |
| **PRD** | [PRD-API-005](../prd/api-005-webhook-events.md) |
| **Projects** | `DiscoSdk` (contracts), `DiscoSdk.AspNetCore` (proposed) |
| **Discord docs** | [events/webhook-events](https://docs.discord.com/developers/events/webhook-events) |
| **Last updated** | 2026-09-29 |

## 1. Summary
This SPEC proposes a new `DiscoSdk.AspNetCore` package. It adds a minimal-API endpoint that:
1. reads the raw body;
2. verifies the Ed25519 signature over `timestamp + body` with the application public key;
3. answers `type: 0` (PING) with 204;
4. for `type: 1`, returns 204 immediately and queues the event to a background dispatcher;
5. the dispatcher parses `event.type` and `event.data` and calls handlers, reusing
   `IDiscordEventHandler<TContext>` and `DiscoFactory` for dependency injection.

Signature verification lives in a shared `DiscordSignatureVerifier`, which the HTTP interactions endpoint
reuses (SPEC-API-009).

## 2. Projects and dependencies
- `DiscoSdk`: new handler and context interfaces (proposed): `IApplicationAuthorizedHandler`,
  `IApplicationDeauthorizedHandler`, `IQuestUserEnrollmentHandler`. Entitlement events reuse the existing
  `IEntitlement*Handler`.
- `DiscoSdk.AspNetCore` (proposed): references `DiscoSdk.Hosting` and `Microsoft.AspNetCore.App`
  (framework reference), plus an Ed25519 provider (see PRD §11).

## 3. Models and contracts
- `WebhookEventPayload { int Version; Snowflake ApplicationId; int Type; WebhookEventBody? Event; }` (proposed)
- `WebhookEventBody { string Type; DateTimeOffset Timestamp; JsonElement Data; }` (proposed)
- `ApplicationAuthorized { int? IntegrationType; IUser User; string[] Scopes; IGuild? Guild; }` (proposed)

## 4. Components
| Type (proposed) | Responsibility |
|---|---|
| `DiscordSignatureVerifier` | `bool Verify(ReadOnlySpan<byte> publicKey, string signatureHex, string timestamp, ReadOnlySpan<byte> body)` |
| `WebhookEventEndpoint` | Minimal-API handler: verify, PING, enqueue, 204. |
| `WebhookEventDispatcher` | `Channel<WebhookEventBody>` consumer mapping `type` to handler interfaces. |
| `ServiceCollectionExtensions.AddDiscoWebhookEvents` / `EndpointRouteBuilderExtensions.MapDiscoWebhookEvents` | Registration. |

## 5. Public API
See the PRD §5 snippet. Handlers are regular `IDiscordEventHandler<TContext>` implementations.

## 6. Discord surface
- Inbound `POST` with headers `X-Signature-Ed25519` and `X-Signature-Timestamp`.
- Twelve event types at the baseline: `APPLICATION_AUTHORIZED`, `APPLICATION_DEAUTHORIZED`,
  `ENTITLEMENT_CREATE/UPDATE/DELETE`, `QUEST_USER_ENROLLMENT`, `LOBBY_MESSAGE_CREATE/UPDATE/DELETE`,
  `GAME_DIRECT_MESSAGE_CREATE/UPDATE/DELETE`.

## 7. Flows
1. Request arrives → buffer the body (limit 1 MiB) → verify the signature, returning 401 on failure.
2. `type == 0` → 204.
3. `type == 1` → write to the channel → 204.
4. The dispatcher dequeues and deserialises `data` by `type`, then runs `HandleAllAsync`.

## 8. Concurrency and lifecycle
The dispatcher is a hosted service. Handler order and `[FireAndForget]` follow SPEC-SDK-02. On shutdown,
the channel is drained within the host's shutdown timeout.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Missing headers | 401 |
| Timestamp older than 5 minutes | 401, to prevent replay (Discord recommends validating the timestamp). |
| Unknown `event.type` | 204, logged at `Debug`, nothing dispatched. |
| Handler throws | Logged; Discord already received the 204. |

## 10. Observability
Counter `discosdk.webhook_events.received` (tags `type`, `result`) and a span per dispatched event (proposed).

## 11. Tests
| Test class (proposed) | Covers |
|---|---|
| `DiscordSignatureVerifierTests` | WE-F03, WE-N01 |
| `WebhookEventEndpointTests` | WE-F01, WE-F02, WE-F04, WE-N03 |
| `WebhookEventDispatcherTests` | WE-F05, WE-F07 – WE-F18 |

## 12. Implementation plan
1. `DiscordSignatureVerifier` plus tests using Discord's documented test vectors.
2. Create the `DiscoSdk.AspNetCore` project, add it to `DiscoSdk.slnx`, and add endpoint and PING handling.
3. Add the payload models and the dispatcher with `APPLICATION_AUTHORIZED` / `APPLICATION_DEAUTHORIZED`.
4. Route entitlement events to the existing handlers.
5. Add the quest event. Add the Social SDK events together with PRD-API-029.

## 13. Decisions and rejected alternatives
- **Separate package** rather than adding ASP.NET Core to `DiscoSdk.Hosting`: gateway-only bots stay free of the web stack.
- **204 before handlers run**: Discord disables endpoints that miss the 3-second budget.
