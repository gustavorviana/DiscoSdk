# PRD-API-011 — Application, role-connection metadata and identity profiles

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-011](../specs/api-011-application.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/application](https://docs.discord.com/developers/resources/application), [resources/application-role-connection-metadata](https://docs.discord.com/developers/resources/application-role-connection-metadata), [resources/application-identity-profile](https://docs.discord.com/developers/resources/application-identity-profile) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
The application object holds the bot's own configuration: install parameters, integration types,
interactions and event URLs, flags, owner and team. Bots read it for owner checks and self-configuration,
and some edit it (description, tags, role-connection verification URL). Linked roles need
role-connection metadata. Game studios using the Social SDK map external identities through identity
profiles.

## 2. Goals
- Read and edit the current application with every documented field.
- Manage linked-role metadata records.
- Read activity instances (embedded apps).
- Map identity-profile endpoints (Social SDK), even though most bots never use them.

## 3. Out of scope
- Application emojis: [PRD-API-017](api-017-emojis.md). SKUs and entitlements:
  [PRD-API-027](api-027-monetization.md). Application commands: [PRD-API-008](api-008-application-commands.md).

## 4. Usage scenarios
- As a bot author, at start I read `GetApplication()` to learn the owner or team for `/admin` checks.
- My deploy script updates the app description and tags through `IApplication.Edit()`.
- I register linked-role metadata ("verified purchases ≥ 5") once, and update each user's role connection
  later (PRD-API-025).

## 5. Desired developer experience

```csharp
var app = await client.GetApplication().ExecuteAsync();
await app.Edit().SetDescription("Moderation bot").ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| AP-F01 | Get Current Application | `GET /applications/@me` | Must | Implemented | `IDiscordClient.GetApplication()` → `ApplicationClient.GetCurrentApplicationAsync` |
| AP-F02 | Edit Current Application | `PATCH /applications/@me` | Must | Implemented | `IApplication.Edit()` → `ApplicationClient.EditCurrentApplicationAsync` |
| AP-F03 | Get Application Activity Instance | `GET /applications/{}/activity-instances/{}` | Could | Implemented | `IDiscordClient.GetActivityInstance()` → `ApplicationClient.GetActivityInstanceAsync` |
| AP-F04 | Update Application Identity Profile | `PATCH /applications/{}/users/{}/identities/{}/profile` | Could | Missing | Social SDK account linking (see §11) |
| AP-F05 | Get Application Identity Profile | `GET /applications/{}/users/{}/identities/{}/profile` | Could | Missing | Social SDK account linking (see §11) |
| AP-F06 | Get Application Identities by User ID | `GET /users/{}/application-identities/{}` | Could | Missing | Social SDK account linking (see §11) |
| AP-F07 | Get Application Identities by External ID | `GET /applications/{}/application-identities/{}/{}` | Could | Missing | Social SDK account linking (see §11) |
| AP-F08 | Delete Application Identity | `POST /users/{}/application-identities/{}/{}/{}/delete` | Could | Missing | Social SDK account linking (see §11) |
| AP-F09 | Get Application Role Connection Metadata Records | `GET /applications/{}/role-connections/metadata` | Must | Implemented | `IDiscordClient.GetRoleConnectionMetadata()` → `ApplicationClient.GetRoleConnectionMetadataAsync` |
| AP-F10 | Update Application Role Connection Metadata Records | `PUT /applications/{}/role-connections/metadata` | Must | Implemented | `IDiscordClient.UpdateRoleConnectionMetadata()` → `ApplicationClient.UpdateRoleConnectionMetadataAsync` |
| AP-F11 | Application object: core fields, flags, install params, integration-types config, URLs, approximate counts | — | Must | Implemented | `IApplication` |
| AP-F12 | Application flags enum (gateway intents limited/enabled, auto-moderation rule badge, …) | — | Should | Implemented | `ApplicationFlags` |
| AP-F13 | Application event webhooks settings (`event_webhooks_status`, `event_webhooks_types`) | — | Could | Partial | `IApplication.EventWebhooksUrl` only; status and types are not modelled |
| AP-F14 | Activity instance object (location, users) | — | Could | Implemented | `IDiscordClient.GetActivityInstance()` (404 → `null`) |
| AP-F15 | Role-connection metadata object and types (integer/datetime/boolean comparisons) | — | Should | Implemented | `IDiscordClient.GetRoleConnectionMetadata()`, `IDiscordClient.UpdateRoleConnectionMetadata()` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| AP-N01 | `ApplicationId` comes from the first READY (`application.id`), so application-scoped routes (commands, interactions, emojis, entitlements) need no extra REST call. The full application object is fetched on demand. |

## 8. Configuration
None.

## 9. Compatibility
Identity-profile support would be additive. It belongs to a Social SDK surface (see PRD-API-029).

## 10. Acceptance criteria
- [x] Get and edit the current application (`ApplicationClientTests`, `ApplicationWrapperTests`).
- [x] Role-connection metadata get and put (`ApplicationClientTests`).
- [x] Activity instance lookup returns `null` on 404 (`ApplicationClientTests`).
- [ ] Identity-profile endpoints (AP-F04 – AP-F08).

## 11. Open questions
- Identity profiles need the provider-issued user id, and are meaningful only with Social SDK account
  linking. Provisional: ship them with the Lobby surface in PRD-API-029.
