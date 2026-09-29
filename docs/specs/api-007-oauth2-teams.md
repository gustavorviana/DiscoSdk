# SPEC-API-007 — OAuth2 and Teams

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-007](../prd/api-007-oauth2-teams.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [topics/oauth2](https://docs.discord.com/developers/topics/oauth2), [topics/teams](https://docs.discord.com/developers/topics/teams) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`IDiscordClient.OAuth2` returns `OAuth2Surface`, which exposes builder actions for the authorize URL, code
exchange and revoke, plus direct actions for refresh, client credentials and `/oauth2/@me`. `OAuth2Client`
sends form-encoded requests through `ClientUtils.SendFormDataAsync`, with a per-request
`AuthenticationHeaderValue` override: HTTP Basic (`client_id:client_secret`) for the token endpoints and
`Bearer` for `/oauth2/@me`. Team data comes from the application object (`ApplicationWrapper` →
`TeamWrapper`, `TeamMemberWrapper`).

## 2. Projects and dependencies
- `DiscoSdk`: `IOAuth2`, `IBuildAuthorizeUrlAction`, `IExchangeAuthorizationCodeAction`, `IRevokeTokenAction`,
  `IAccessTokenResponse`, `ICurrentAuthorizationInfo`, `OAuth2Scope`, `OAuth2ResponseType`, `OAuth2Prompt`,
  `ITeam`, `ITeamMember`, `TeamMembershipState`.
- `DiscoSdk.Hosting`: `OAuth2Surface`, `OAuth2Client`, `ClientUtils`, `CurrentAuthorizationInfoWrapper`,
  `TeamWrapper`, `TeamMemberWrapper`.

## 3. Models and contracts
- `IAccessTokenResponse { AccessToken, TokenType, ExpiresIn, RefreshToken?, Scope }`. There is no
  `webhook` or `guild` field (OA-F10).
- `ICurrentAuthorizationInfo { Application, Scopes, Expires, User? }`.

## 4. Components
| Type | Responsibility |
|---|---|
| `OAuth2Surface` | `IOAuth2` implementation; binds the client id from the current application. |
| `OAuth2Client` | `ExchangeAuthorizationCodeAsync`, `RefreshAccessTokenAsync`, `GetClientCredentialsTokenAsync`, `RevokeTokenAsync`, `GetCurrentAuthorizationInfoAsync`. |
| Builder actions | Validate the required fields (code, redirect URI, secret) before sending. |

## 5. Public API
See PRD §5. `IBuildAuthorizeUrlAction.Build()` returns a string and makes no network call.

## 6. Discord surface
- `https://discord.com/oauth2/authorize`, `POST /oauth2/token`, `POST /oauth2/token/revoke`,
  `GET /oauth2/@me`.
- `GET /oauth2/applications/@me` is not used.

## 7. Flows
**Code exchange**
1. Validate `code`, `redirect_uri` and `client_secret`.
2. Form body: `grant_type=authorization_code`, `code`, `redirect_uri`.
3. Basic auth → `POST /oauth2/token` → `IAccessTokenResponse`.

## 8. Concurrency and lifecycle
Stateless. Tokens are not stored by the SDK.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Invalid code or redirect mismatch | `DiscordApiException` 400 (`invalid_grant`, OAuth-style body; `DiscordCode` is null). |
| Missing required builder field | `InvalidOperationException` before sending. |

## 10. Observability
REST metrics only. Secrets are never logged.

## 11. Tests
| Test class | Covers |
|---|---|
| `BuildAuthorizeUrlActionTests` | OA-F05, OA-F06, OA-F07, OA-F09, OA-F15 |
| `OAuth2ClientTests`, `OAuth2BuilderActionsTests` | OA-F01 – OA-F04, OA-F16, OA-F17, OA-N01 |
| `CurrentAuthorizationInfoWrapperTests` | OA-F14 |
| `ApplicationWrapperTests` | TM-F01, TM-F02 |

## 12. History
| Commit | Change |
|---|---|
| `225dad3` | OAuth2 token flow and cohesive surfaces under sub-interfaces. |

Next steps:
1. Add `SetIntegrationType` to the authorize builder.
2. Add `webhook` and `guild` to the token response.
3. Add a `TeamMemberRole` enum.
4. Consider a bearer-token user client (PRD §11).

## 13. Decisions and rejected alternatives
- **Builder actions for multi-field requests** rather than long parameter lists: required fields are validated
  in one place, and new parameters stay additive.
