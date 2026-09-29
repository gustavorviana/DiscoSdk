# SPEC-API-011 — Application, role-connection metadata and identity profiles

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-011](../prd/api-011-application.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/application](https://docs.discord.com/developers/resources/application), [resources/application-role-connection-metadata](https://docs.discord.com/developers/resources/application-role-connection-metadata), [resources/application-identity-profile](https://docs.discord.com/developers/resources/application-identity-profile) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`ApplicationClient` implements the application routes. `IDiscordClient.GetApplication()` returns
`ApplicationWrapper` (`IApplication`), whose `Edit()` returns an `EditApplicationAction` builder.
Role-connection metadata and activity instances are exposed directly on `IDiscordClient`. Identity-profile
routes are not implemented.

## 2. Projects and dependencies
- `DiscoSdk/Models/Applications/*`: `IApplication`, `ITeam`, `IApplicationInstallParams`,
  `IApplicationIntegrationTypeConfiguration`, `IApplicationRoleConnectionMetadata`, `ActivityInstance`.
- `DiscoSdk.Hosting`: `ApplicationClient`, `ApplicationWrapper`, `TeamWrapper`, `EditApplicationAction`.

## 3. Models and contracts
The wire `Application` maps every documented field except `event_webhooks_status` and
`event_webhooks_types`. `ActivityInstance` carries `application_id`, `instance_id`, `launch_id`, `location`
and `users`.

## 4. Components
| Type | Responsibility |
|---|---|
| `ApplicationClient` | `GetCurrentApplicationAsync`, `EditCurrentApplicationAsync`, `GetActivityInstanceAsync` (404 → `null`), `GetRoleConnectionMetadataAsync`, `UpdateRoleConnectionMetadataAsync`, plus emoji/SKU/entitlement routes (other PRDs). |
| `EditApplicationAction` | Sends only modified fields (description, icon, cover, tags, URLs, install params, flags, integration-types config). |

## 5. Public API
`IDiscordClient.GetApplication()`, `IApplication.Edit()`, `IDiscordClient.GetActivityInstance()`,
`IDiscordClient.GetRoleConnectionMetadata()`, `IDiscordClient.UpdateRoleConnectionMetadata(records)`.

## 6. Discord surface
- `GET` and `PATCH /applications/@me`.
- `GET /applications/{id}/activity-instances/{instance}`.
- `GET` and `PUT /applications/{id}/role-connections/metadata`.

## 7. Flows
`GetApplication()` → `GET /applications/@me` → `ApplicationWrapper`. The id is also cached from READY
(`DiscordClient.ApplicationId`).

## 8. Concurrency and lifecycle
The application object is not cached; each call fetches it again.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Unknown activity instance | Returns `null` (404 is swallowed). |
| More than 5 role-connection metadata records | Discord 400 → `InvalidRequestBodyException`. |

## 10. Observability
REST metrics only.

## 11. Tests
| Test class | Covers |
|---|---|
| `ApplicationClientTests` | AP-F01 – AP-F03, AP-F09, AP-F10, AP-F14, AP-F15 |
| `ApplicationWrapperTests` | AP-F11, AP-F12 |

## 12. History
| Commit | Change |
|---|---|
| `3bfa484` | Get Application Activity Instance endpoint and model. |
| `225dad3` | Cohesive surfaces (application-level sub-interfaces). |

Next steps: model the event-webhook settings, and add identity-profile routes together with PRD-API-029.

## 13. Decisions and rejected alternatives
- **No application cache**: the object changes rarely, but edits from the developer portal would make a
  cached copy stale. Callers cache it if they need to.
