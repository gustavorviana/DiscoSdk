# SPEC-API-026 — Webhooks

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-026](../prd/api-026-webhooks.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/webhook](https://docs.discord.com/developers/resources/webhook) |
| **Last updated** | 2026-09-29 |

## 1. Summary
There are two entry points:
- **Webhook management with bot auth**: `IDiscordClient.Webhooks` (`WebhooksSurface`: `Create`, `ListForChannel`, `Get`), `IGuild.GetWebhooks()`, and `WebhookWrapper` (`IWebhook`: `Modify`, `Delete`), all through `WebhookClient`.
- **Token-based execution**: `DiscordWebhookClientBuilder` (URL, or id + token) produces an `IDiscordWebhookClient` backed by `WebhookMessageClient` and a bot-less `DiscordRestClient`. The same `WebhookMessageClient` serves interaction follow-ups.

## 2. Projects and dependencies
`DiscoSdk` (`IWebhook`, `IWebhooks`, `IWebhookInfo`, `IDiscordWebhookClient`, `IWebhookSendMessageRestAction`, `IWebhookEditMessageRestAction`, `WebhookType`), and `DiscoSdk.Hosting` (`WebhookClient`, `WebhookMessageClient`, `WebhooksSurface`, `WebhookWrapper`, `DiscordWebhookClient`, `DiscordWebhookClientBuilder`).

## 3. Models and contracts
The wire webhook maps every documented field, including `source_guild`, `source_channel` and `url`.

## 4. Components
| Type | Responsibility |
|---|---|
| `WebhookClient` | Create, channel/guild list, get (with and without token), modify (with and without token), delete (with and without token). |
| `WebhookMessageClient` | Execute (`wait`, `thread_id`), get/edit/delete messages, `@original` (interactions), `GetInfoAsync`. |
| `DiscordWebhookClientBuilder` | Parses `https://discord.com/api/webhooks/{id}/{token}`, validates the webhook through `GetInfoAsync`, and returns the client. |

## 5. Public API
`IDiscordClient.Webhooks.Create/ListForChannel/Get`, `IGuild.GetWebhooks()`, `IWebhook.Modify()` / `Delete()`, `DiscordWebhookClientBuilder` → `IDiscordWebhookClient.Send/EditMessage/GetMessageById/DeleteMessage`.

## 6. Discord surface
11 webhook routes (Slack and GitHub excluded), plus the 4 message routes owned by PRD-API-009. Management requires `MANAGE_WEBHOOKS`.

## 7. Flows
Standalone: builder → `GET /webhooks/{id}/{token}` (validate) → `POST /webhooks/{id}/{token}?wait=true` → `IWebhookMessage`.

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Invalid webhook URL format | `InvalidOperationException` from the builder. |
| Deleted webhook (404) | `DiscordResourceNotFoundException`. The client is not marked dead (see RL-F09). |
| Components on a non-application webhook | Discord ignores them without `with_components=true` (not supported). |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `WebhookClientTests` | WH-F01 – WH-F09 |
| `CreateWebhookActionTests`, `ModifyWebhookActionTests`, `WebhookWrapperTests` | WH-F01, WH-F06, WH-F12 – WH-F14, WH-F17 |
| `WebhookMessageClientTests`, `WebhookMessageWrapperTests` | WH-F15, WH-F16 |

Implemented or Partial requirements without a covering test: WH-N01 – WH-N02.

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Webhook REST. |
| `15c4af6` | `WithReason` on webhook mutations. |

Next steps: add the `with_components` flag, and expose token-auth modify and delete on `IDiscordWebhookClient`.

## 13. Decisions and rejected alternatives
- **A standalone client that needs no bot**: CI and integrations can post without a gateway session or a bot token.
