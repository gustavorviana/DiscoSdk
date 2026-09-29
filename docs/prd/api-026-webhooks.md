# PRD-API-026 — Webhooks

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-026](../specs/api-026-webhooks.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/webhook](https://docs.discord.com/developers/resources/webhook) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Webhooks post messages without a bot session, for example CI notifications or bridging, and use the webhook's own token. Bots also manage webhooks, both with bot auth (create, list, modify, delete) and with the webhook token. Message execution routes are shared with interaction follow-ups.

## 2. Goals
- Webhook management with bot auth and with the webhook token.
- A standalone `IDiscordWebhookClient` built from a webhook URL, with send, edit, get and delete, and thread targeting.
- Webhook message routes are reused by interaction follow-ups (PRD-API-009).

## 3. Out of scope
- The four webhook message routes (`POST /webhooks/{id}/{token}`, `GET` / `PATCH` / `DELETE …/messages/{id}`) are owned by [PRD-API-009](api-009-interactions.md) (IN-F15 – IN-F18), because Discord documents them on both pages. This PRD covers webhook usage of them in WH-F15.
- `WEBHOOKS_UPDATE`: [PRD-API-004](api-004-gateway-events.md).

## 4. Usage scenarios
- As a bot author, my CI posts build results through a webhook URL, with no bot token.
- My bot creates a webhook per channel for a bridge and posts with custom usernames and avatars.

## 5. Desired developer experience

```csharp
var webhook = await new DiscordWebhookClientBuilder(webhookUrl).BuildAsync();
await webhook.Send().SetContent("Build #512 passed").ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| WH-F01 | Create Webhook | `POST /channels/{}/webhooks` | Must | Implemented | `IWebhooks.Create()` → `WebhookClient.CreateAsync` |
| WH-F02 | Get Channel Webhooks | `GET /channels/{}/webhooks` | Must | Implemented | `IWebhooks.ListForChannel()` → `WebhookClient.GetChannelWebhooksAsync` |
| WH-F03 | Get Guild Webhooks | `GET /guilds/{}/webhooks` | Must | Implemented | `IGuild.GetWebhooks()` → `WebhookClient.GetGuildWebhooksAsync` |
| WH-F04 | Get Webhook | `GET /webhooks/{}` | Must | Implemented | `IWebhooks.Get()` → `WebhookClient.GetAsync` |
| WH-F05 | Get Webhook with Token | `GET /webhooks/{}/{}` | Must | Implemented | `IWebhooks.Get()` → `WebhookClient.GetWithTokenAsync`; `DiscordWebhookClientBuilder.BuildAsync` → `WebhookMessageClient.GetInfoAsync` |
| WH-F06 | Modify Webhook | `PATCH /webhooks/{}` | Must | Implemented | `IWebhook.Modify()` → `WebhookClient.ModifyAsync` |
| WH-F07 | Modify Webhook with Token | `PATCH /webhooks/{}/{}` | Should | Partial | `WebhookClient.ModifyWithTokenAsync` exists, but no public API reaches it (neither `IDiscordWebhookClient` nor `IWebhooks`) |
| WH-F08 | Delete Webhook | `DELETE /webhooks/{}` | Must | Implemented | `IWebhook.Delete()` → `WebhookClient.DeleteAsync` |
| WH-F09 | Delete Webhook with Token | `DELETE /webhooks/{}/{}` | Should | Partial | `WebhookClient.DeleteWithTokenAsync` exists, but no public API reaches it |
| WH-F10 | Execute Slack-Compatible Webhook | `POST /webhooks/{}/{}/slack` | Could | Excluded | Services post Slack-format payloads to Discord directly; see §11 |
| WH-F11 | Execute GitHub-Compatible Webhook | `POST /webhooks/{}/{}/github` | Could | Excluded | Services post GitHub payloads to Discord directly; see §11 |
| WH-F12 | Webhook type `Incoming` (1) | `webhook-type:1` | Must | Implemented | `WebhookType.Incoming` |
| WH-F13 | Webhook type `Channel Follower` (2) | `webhook-type:2` | Must | Implemented | `WebhookType.ChannelFollower` |
| WH-F14 | Webhook type `Application` (3) | `webhook-type:3` | Must | Implemented | `WebhookType.Application` |
| WH-F15 | Execute webhook and manage webhook messages (username/avatar override, `wait`, `thread_id`, `thread_name`, attachments, components with `with_components`) | — | Must | Partial | `IDiscordWebhookClient.Send()` (`SetUsername`, `SetAvatarUrl`, `SetThreadName`, `wait`, `thread_id`), `EditMessage()`, `GetMessageById()`, `DeleteMessage()` → `WebhookMessageClient` (routes owned by PRD-API-009); the `with_components` query flag, which non-application webhooks need to send components, is not supported |
| WH-F16 | Standalone webhook client from a URL or id + token | — | Must | Implemented | `DiscordWebhookClientBuilder`, `IDiscordWebhookClient` |
| WH-F17 | Webhook object (type, guild, channel, user, name, avatar, token, application, source guild/channel, url) | — | Must | Implemented | `IWebhook`, `IWebhookInfo` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| WH-N01 | Webhook tokens are credentials: never logged, and masked in exceptions. |
| WH-N02 | The standalone client does not need a bot token or gateway connection. |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] Webhook management (`WebhookClientTests`, `CreateWebhookActionTests`, `ModifyWebhookActionTests`, `WebhookWrapperTests`).
- [x] Standalone client send/edit/delete (`WebhookMessageClientTests`, `WebhookMessageWrapperTests`).
- [ ] Token-auth modify/delete are public (WH-F07, WH-F09).

## 11. Open questions
- Slack and GitHub compatible execution: needed? Provisional: Excluded, since services post to those URLs directly and a .NET SDK adds no value.
