# PRD-API-007 — OAuth2 and Teams

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-007](../specs/api-007-oauth2-teams.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [topics/oauth2](https://docs.discord.com/developers/topics/oauth2), [topics/teams](https://docs.discord.com/developers/topics/teams) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Bots need OAuth2 for three things:
- building install links, for guild or user installs;
- running dashboards that act on a user's behalf (authorization code, refresh, revoke);
- linked roles and `guilds.join`.

Hand-building authorize URLs and token requests (form-encoded, HTTP Basic auth) is error-prone. Team data
matters for owner checks on bots owned by a team.

## 2. Goals
- A fluent authorize-URL builder that covers every documented query parameter.
- Token exchange, refresh, client credentials and revoke, with typed responses.
- Team and team-member models on the application object.

## 3. Out of scope
- Hosting a redirect endpoint: that belongs to the bot author's web app.
- Linked-role metadata endpoints: [PRD-API-011](api-011-application.md). User role connections:
  [PRD-API-025](api-025-users.md).

## 4. Usage scenarios
- As a bot author, I generate an install link with the `bot` and `applications.commands` scopes, preset
  permissions and a guild pre-selected.
- My dashboard exchanges `?code=` for tokens, refreshes them before they expire and revokes them on logout.
- When checking "is this user an owner?", I include team members with the admin role.

## 5. Desired developer experience

```csharp
var url = client.OAuth2.BuildAuthorizeUrl()
    .SetScopes(OAuth2Scope.Bot, OAuth2Scope.ApplicationsCommands)
    .SetPermissions((ulong)(DiscordPermission.SendMessages | DiscordPermission.EmbedLinks))
    .SetGuildId(guildId)
    .Build();

var tokens = await client.OAuth2.ExchangeAuthorizationCode()
    .SetClientSecret(secret).SetCode(code).SetRedirectUri(redirect)
    .ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| OA-F01 | Authorization code grant: exchange the code for tokens | — | Must | Implemented | `IOAuth2.ExchangeAuthorizationCode()` → `OAuth2Client.ExchangeAuthorizationCodeAsync` |
| OA-F02 | Refresh token grant | — | Must | Implemented | `IOAuth2.RefreshAccessToken()` → `OAuth2Client.RefreshAccessTokenAsync` |
| OA-F03 | Client credentials grant (bot owner's bearer token) | — | Should | Implemented | `IOAuth2.GetClientCredentialsToken()` → `OAuth2Client.GetClientCredentialsTokenAsync` |
| OA-F04 | Token revocation (`access_token` / `refresh_token` hint) | — | Should | Implemented | `IOAuth2.RevokeToken()` → `OAuth2Client.RevokeTokenAsync` |
| OA-F05 | Implicit grant (`response_type=token`) | — | Could | Implemented | `IBuildAuthorizeUrlAction.SetResponseType` (`OAuth2ResponseType.Token`) |
| OA-F06 | `state` anti-CSRF parameter and `prompt` | — | Must | Implemented | `IBuildAuthorizeUrlAction.SetState`, `IBuildAuthorizeUrlAction.SetPrompt` |
| OA-F07 | Bot authorization flow (`bot` scope, `permissions`, `guild_id`, `disable_guild_select`) | — | Must | Implemented | `IBuildAuthorizeUrlAction.SetPermissions`, `IBuildAuthorizeUrlAction.SetGuildId`, `IBuildAuthorizeUrlAction.SetDisableGuildSelect` |
| OA-F08 | `integration_type` in the authorize URL (guild install vs user install) | — | Should | Missing | No setter on `IBuildAuthorizeUrlAction` |
| OA-F09 | Scope catalogue | — | Must | Implemented | `OAuth2Scope` (27 constants) |
| OA-F10 | `webhook.incoming`: token response carries the created webhook | — | Could | Missing | `IAccessTokenResponse` has no `Webhook` (nor `Guild` for `bot`-scope code grants) |
| OA-F11 | Calling the API with a user's bearer token | — | Should | Partial | Bearer override exists internally (`ClientUtils.SendFormDataAsync`, command permissions, `/oauth2/@me`), but there is no general user-token client |
| OA-F12 | Two-factor requirement for privileged actions on MFA-enabled guilds | — | Should | Implemented | Surfaces as `DiscordApiException` (JSON code 60003) from the owner's account settings; nothing to do client-side |
| TM-F01 | Team object (id, icon, name, owner, members) | — | Should | Implemented | `IApplication.Team` (`ITeam`) |
| TM-F02 | Team member (membership state, user, role: admin/developer/read_only) | — | Should | Implemented | `ITeamMember` (`Role` is a raw string; no enum) |
| OA-F13 | Get Current Bot Application Information | `GET /oauth2/applications/@me` | Should | Missing | `IDiscordClient.GetApplication()` uses `GET /applications/@me` instead (same object; PRD-API-011) |
| OA-F14 | Get Current Authorization Information | `GET /oauth2/@me` | Must | Implemented | `IOAuth2.GetCurrentAuthorizationInfo()` → `OAuth2Client.GetCurrentAuthorizationInfoAsync` |
| OA-F15 | Base authorization URL | `oauth2:AUTHORIZE` | Must | Implemented | `IOAuth2.BuildAuthorizeUrl()` → `IBuildAuthorizeUrlAction.Build` |
| OA-F16 | Token URL | `POST /oauth2/token` | Must | Implemented | `IOAuth2.ExchangeAuthorizationCode()`, `IOAuth2.RefreshAccessToken()`, `IOAuth2.GetClientCredentialsToken()` → `OAuth2Client` |
| OA-F17 | Token Revocation URL | `POST /oauth2/token/revoke` | Must | Implemented | `IOAuth2.RevokeToken()` → `OAuth2Client.RevokeTokenAsync` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| OA-N01 | Token and revoke requests are `application/x-www-form-urlencoded` with HTTP Basic client authentication, as Discord requires (JSON is rejected). |
| OA-N02 | Client secrets and tokens are never logged. |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| Client id | from the current application | snowflake | `IBuildAuthorizeUrlAction` (implicit) |
| Response type | `Code` | `Code`, `Token` | `IBuildAuthorizeUrlAction.SetResponseType` |

## 9. Compatibility
Adding `SetIntegrationType` and optional response fields is additive.

## 10. Acceptance criteria
- [x] Authorize URLs include every configured parameter, URL-encoded (`BuildAuthorizeUrlActionTests`).
- [x] Token requests are form-encoded with Basic authentication (`OAuth2ClientTests`, `OAuth2BuilderActionsTests`).
- [x] `/oauth2/@me` parses application, scopes, expiry and user (`CurrentAuthorizationInfoWrapperTests`).
- [ ] `integration_type` is supported (OA-F08), and `GET /oauth2/applications/@me` is exposed (OA-F13).

## 11. Open questions
- Should DiscoSdk offer a user-scoped client (`DiscordUserClient`) for bearer tokens? Provisional: yes, a
  lightweight client that reuses `DiscordRestClient` with a bearer header, exposing only user-token
  endpoints (`/users/@me/guilds`, `guilds.join`, role connections).
