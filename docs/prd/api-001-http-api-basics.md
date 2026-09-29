# PRD-API-001 — HTTP API basics

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-001](../specs/api-001-http-api-basics.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [reference](https://docs.discord.com/developers/reference), [topics/opcodes-and-status-codes](https://docs.discord.com/developers/topics/opcodes-and-status-codes) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Every other area of the SDK sits on the same HTTP conventions: bot-token authentication, a versioned base
URL, a Discord-compliant User-Agent, JSON bodies, snowflake IDs, typed error responses, multipart uploads
and CDN URLs. If any of these is wrong, every endpoint is wrong in the same way, and bot authors end up
parsing raw error JSON or building CDN URLs by hand.

## 2. Goals
- All REST traffic goes to an explicit API version, authenticated with `Bot <token>`, with a compliant
  User-Agent.
- Failures surface as typed exceptions that carry Discord's JSON error code and field-level validation
  errors.
- IDs, timestamps, images and files have first-class types (`Snowflake`, `DiscordImageBuffer`,
  `DiscordImageUrl`), so bot authors never handle raw strings.
- Message-formatting helpers cover every markup Discord documents.

## 3. Out of scope
- Rate limiting and retries: [PRD-API-002](api-002-rate-limits.md).
- Gateway opcodes and close codes (listed on the same Discord page): [PRD-API-003](api-003-gateway-connection.md).
  Voice opcodes and close codes: [PRD-API-028](api-028-voice.md).
- The `X-Audit-Log-Reason` header: [PRD-API-012](api-012-audit-log.md).

## 4. Usage scenarios
- As a bot author, when an edit fails because I lack a permission, I catch `InsufficientPermissionException`
  and read `DiscordCode == 50013`, not a string.
- When a form field is invalid, I read `ValidationErrors` on `InvalidRequestBodyException` to see which
  field failed.
- When I show a user, I read `IUser.Avatar.Url` without knowing the CDN path rules.
- When I write a reminder message, I append `<t:…:R>` through `MessageTextBuilder.AppendTimestamp`.

## 5. Desired developer experience

```csharp
try
{
    await message.Pin().ExecuteAsync();
}
catch (InsufficientPermissionException ex) when (ex.DiscordCode == 50013)
{
    logger.LogWarning("Missing permissions: {Message}", ex.Message);
}

var created = message.Id.CreatedAt;                     // Snowflake → DateTimeOffset
var avatar = user.Avatar?.Url;                          // CDN URL, animated-aware
var text = new MessageTextBuilder()
    .AppendMention(Mention.FromUser(user, ping: true))
    .Append(" meeting starts ")
    .AppendTimestamp(start, TimestampFormat.RelativeTime)
    .ToString();
```

## 6. Capabilities

| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| HB-F01 | Bot token authentication (`Authorization: Bot <token>`) | — | Must | Implemented | `DiscordClientBuilder.Create` → `DiscordRestClient` (default `Authorization` header) |
| HB-F02 | API versioning: every request targets an explicit version | — | Must | Partial | `DiscordClientBuilder.Build` passes `https://discord.com/api/v10` **without a trailing slash**; relative routes (`channels/…`) resolve per RFC 3986 to `/api/channels/…`, i.e. Discord's unversioned default (v6, deprecated). `DiscordWebhookClientBuilder` uses the unversioned `https://discord.com/api/`. |
| HB-F03 | Compliant REST User-Agent `DiscordBot ($url, $version)` | — | Must | Partial | `DiscordRestClient` sends `DiscoSdk/<assembly version>` (not the documented form). The gateway uses the compliant `DiscordClientConfig.DefaultGatewayUserAgent`. |
| HB-F04 | JSON request/response bodies (`application/json`) | — | Must | Implemented | `DiscordRestClient.SendAsync` with `JsonOptions` |
| HB-F05 | Typed errors from HTTP status + JSON error code | — | Must | Implemented | `DiscordApiException` (`StatusCode`, `DiscordCode`), `InvalidTokenException`, `InsufficientPermissionException`, `DiscordResourceNotFoundException`, `InvalidRequestBodyException` → `DiscordErrorParser` |
| HB-F06 | Field-level validation errors (`errors` object of 50035) | — | Must | Implemented | `DiscordApiException.ValidationErrors` (`DiscordValidationError`) → `DiscordErrorParser.Parse` |
| HB-F07 | HTTP response codes (200/201/204/304/400/401/403/404/405/429/502/5xx) | — | Must | Implemented | `DiscordRestClient` (204 → no body, 429 → PRD-API-002, 5xx → `TransientRetryPolicy`) |
| HB-F08 | JSON error code catalogue (~270 codes) | — | Should | Partial | Codes are surfaced as raw integers on `DiscordApiException.DiscordCode`; only 40001, 50014, 50001, 50013, 10001–10068 and 50035 are mapped to types. No named constants. |
| HB-F09 | Snowflake type (parse, compare, timestamp, worker/process/increment) | — | Must | Implemented | `Snowflake` (`CreatedAt`, `WorkerId`, `ProcessId`, `Increment`) |
| HB-F10 | Snowflake pagination helpers (`before`/`after` from a timestamp) | — | Could | Missing | No `Snowflake.FromDateTimeOffset`; pagination actions take IDs only. |
| HB-F11 | ID serialisation as JSON strings | — | Must | Implemented | `SnowflakeConverter` |
| HB-F12 | ISO8601 timestamps; nullable vs optional fields | — | Must | Implemented | `DateTimeOffset` models; optional request fields tracked by builders (only modified keys are sent, e.g. `ManagerWrapper.ModifiedKeys`) |
| HB-F13 | Boolean query strings (`true`/`false`) | — | Must | Implemented | Query builders in `UserClient.GetCurrentGuildsAsync`, `InviteClient.GetAsync` |
| HB-F14 | Image data URIs for uploads (avatars, icons, emojis) | — | Must | Implemented | `DiscordImageBuffer` (`ToBase64`, `LoadFile`); the data URI is assembled per action (`EditGuildAction`, `RoleAction`) |
| HB-F15 | File uploads (`multipart/form-data`, `payload_json`, `files[n]`, attachment metadata) | — | Must | Implemented | `DiscordRestClient.SendMultipartAsync` ← `MessageClient.CreateAsync` / `EditAsync`, `SendFormDataAsync` for stickers and soundboard |
| HB-F16 | Editing message attachments (keep/remove by `attachments[].id`) | — | Should | Implemented | `IMessage.ToBuilder` full-fidelity fork (commit 70f78e8), `MessageClient.EditAsync` |
| HB-F17 | Signed attachment CDN URLs (`ex`/`is`/`hm` parameters) | — | Could | Missing | Attachment URLs are exposed verbatim; no expiry parsing or refresh helper. |
| HB-F18 | Message formatting: user | `format:USER` | Must | Implemented | `Mention.FromUser` → `MessageTextBuilder.AppendMention` (`<@id>`; the legacy `<@!id>` form is not emitted, which Discord allows) |
| HB-F19 | Message formatting: channel | `format:CHANNEL` | Must | Partial | `Mention.FromChannel` exists, but `Mention.ToString()` has no `MentionType.Channel` branch and renders `@everyone` instead of `<#id>` |
| HB-F20 | Message formatting: role | `format:ROLE` | Must | Implemented | `Mention.FromRole` → `MessageTextBuilder.AppendMention` (`<@&id>`) |
| HB-F21 | Message formatting: game profile | `format:GAME_PROFILE` | Could | Missing | — |
| HB-F22 | Message formatting: slash command | `format:SLASH_COMMAND` | Could | Missing | — |
| HB-F23 | Message formatting: slash command with subcommand | `format:SLASH_COMMAND_WITH_SUBCOMMAND` | Could | Missing | — |
| HB-F24 | Message formatting: slash command with subcommand group | `format:SLASH_COMMAND_WITH_SUBCOMMAND_GROUP` | Could | Missing | — |
| HB-F25 | Message formatting: standard emoji | `format:STANDARD_EMOJI` | Should | Implemented | Unicode passes through `MessageTextBuilder.Append`; `Emoji.Name` holds the character |
| HB-F26 | Message formatting: custom emoji | `format:CUSTOM_EMOJI` | Should | Missing | `Emoji.ToString()` yields `name:id` (the reaction-route form), not `<:name:id>` |
| HB-F27 | Message formatting: animated custom emoji | `format:ANIMATED_CUSTOM_EMOJI` | Should | Missing | No `<a:name:id>` helper |
| HB-F28 | Message formatting: unix timestamp | `format:UNIX_TIMESTAMP` | Should | Implemented | `MessageTextBuilder.AppendTimestamp` (always emits a style; default `f`) |
| HB-F29 | Message formatting: styled unix timestamp | `format:STYLED_UNIX_TIMESTAMP` | Should | Partial | `TimestampFormat` has `t T d D f F R`; the newer `s` and `S` styles are missing |
| HB-F30 | Message formatting: guild navigation | `format:GUILD_NAVIGATION` | Could | Missing | — |
| HB-F31 | CDN endpoint: custom emoji | `cdn:CUSTOM_EMOJI` | Should | Missing | No CDN URL on `IEmoji`/`Emoji` |
| HB-F32 | CDN endpoint: guild icon | `cdn:GUILD_ICON` | Must | Implemented | `IGuild.Icon` → `DiscordImageUrl.ParseIcon` |
| HB-F33 | CDN endpoint: guild splash | `cdn:GUILD_SPLASH` | Should | Implemented | `IGuild.Splash` → `DiscordImageUrl.ParseSplash` |
| HB-F34 | CDN endpoint: guild discovery splash | `cdn:GUILD_DISCOVERY_SPLASH` | Should | Implemented | `IGuild.DiscoverySplash` → `DiscordImageUrl.ParseDiscoverySplash` |
| HB-F35 | CDN endpoint: guild banner | `cdn:GUILD_BANNER` | Should | Implemented | `IGuild.Banner` → `DiscordImageUrl.ParseBanner` |
| HB-F36 | CDN endpoint: user banner | `cdn:USER_BANNER` | Should | Implemented | `IUser.Banner`, `IUser.EffectiveBannerUrl` → `DiscordImageUrl.ParseBanner` |
| HB-F37 | CDN endpoint: default user avatar | `cdn:DEFAULT_USER_AVATAR` | Must | Partial | `DiscordImageUrl.ParseAvatar` uses `user_id % 5` and `IUser.EffectiveAvatarUrl` uses `discriminator % 5`; Discord specifies `(user_id >> 22) % 6` for users on the new username system |
| HB-F38 | CDN endpoint: user avatar | `cdn:USER_AVATAR` | Must | Implemented | `IUser.Avatar`, `IUser.EffectiveAvatarUrl` → `DiscordImageUrl.ParseAvatar` |
| HB-F39 | CDN endpoint: guild member avatar | `cdn:GUILD_MEMBER_AVATAR` | Should | Implemented | `IMember.AvatarUrl`, `IMember.EffectiveAvatarUrl` |
| HB-F40 | CDN endpoint: avatar decoration | `cdn:AVATAR_DECORATION` | Could | Missing | — |
| HB-F41 | CDN endpoint: application icon | `cdn:APPLICATION_ICON` | Should | Missing | `IApplication` exposes the icon hash only |
| HB-F42 | CDN endpoint: application cover | `cdn:APPLICATION_COVER` | Could | Missing | — |
| HB-F43 | CDN endpoint: application asset | `cdn:APPLICATION_ASSET` | Could | Missing | — |
| HB-F44 | CDN endpoint: achievement icon | `cdn:ACHIEVEMENT_ICON` | Could | Missing | — |
| HB-F45 | CDN endpoint: store page asset | `cdn:STORE_PAGE_ASSET` | Could | Missing | — |
| HB-F46 | CDN endpoint: sticker pack banner | `cdn:STICKER_PACK_BANNER` | Could | Missing | — |
| HB-F47 | CDN endpoint: team icon | `cdn:TEAM_ICON` | Could | Missing | — |
| HB-F48 | CDN endpoint: sticker | `cdn:STICKER` | Should | Missing | No CDN URL on `ISticker` (PNG/APNG/Lottie/GIF by format type) |
| HB-F49 | CDN endpoint: role icon | `cdn:ROLE_ICON` | Should | Missing | `IRole` exposes the icon hash only |
| HB-F50 | CDN endpoint: guild scheduled event cover | `cdn:GUILD_SCHEDULED_EVENT_COVER` | Could | Missing | — |
| HB-F51 | CDN endpoint: guild member banner | `cdn:GUILD_MEMBER_BANNER` | Could | Missing | — |
| HB-F52 | CDN endpoint: guild tag badge | `cdn:GUILD_TAG_BADGE` | Could | Missing | — |
| HB-F53 | Locale `id` (Indonesian) | `locale:id` | Should | Implemented | `DiscordLocales` |
| HB-F54 | Locale `da` (Danish) | `locale:da` | Should | Implemented | `DiscordLocales` |
| HB-F55 | Locale `de` (German) | `locale:de` | Should | Implemented | `DiscordLocales` |
| HB-F56 | Locale `en-GB` (English, UK) | `locale:en-GB` | Should | Implemented | `DiscordLocales` |
| HB-F57 | Locale `en-US` (English, US) | `locale:en-US` | Should | Implemented | `DiscordLocales` |
| HB-F58 | Locale `es-ES` (Spanish) | `locale:es-ES` | Should | Implemented | `DiscordLocales` |
| HB-F59 | Locale `es-419` (Spanish, LATAM) | `locale:es-419` | Should | Missing | Not in `DiscordLocales`, so `es-419` localizations are rejected by `DiscordLocales.Has` |
| HB-F60 | Locale `fr` (French) | `locale:fr` | Should | Implemented | `DiscordLocales` |
| HB-F61 | Locale `hr` (Croatian) | `locale:hr` | Should | Implemented | `DiscordLocales` |
| HB-F62 | Locale `it` (Italian) | `locale:it` | Should | Implemented | `DiscordLocales` |
| HB-F63 | Locale `lt` (Lithuanian) | `locale:lt` | Should | Implemented | `DiscordLocales` |
| HB-F64 | Locale `hu` (Hungarian) | `locale:hu` | Should | Implemented | `DiscordLocales` |
| HB-F65 | Locale `nl` (Dutch) | `locale:nl` | Should | Implemented | `DiscordLocales` |
| HB-F66 | Locale `no` (Norwegian) | `locale:no` | Should | Implemented | `DiscordLocales` |
| HB-F67 | Locale `pl` (Polish) | `locale:pl` | Should | Implemented | `DiscordLocales` |
| HB-F68 | Locale `pt-BR` (Portuguese, Brazilian) | `locale:pt-BR` | Should | Implemented | `DiscordLocales` |
| HB-F69 | Locale `ro` (Romanian, Romania) | `locale:ro` | Should | Implemented | `DiscordLocales` |
| HB-F70 | Locale `fi` (Finnish) | `locale:fi` | Should | Implemented | `DiscordLocales` |
| HB-F71 | Locale `sv-SE` (Swedish) | `locale:sv-SE` | Should | Implemented | `DiscordLocales` |
| HB-F72 | Locale `vi` (Vietnamese) | `locale:vi` | Should | Implemented | `DiscordLocales` |
| HB-F73 | Locale `tr` (Turkish) | `locale:tr` | Should | Implemented | `DiscordLocales` |
| HB-F74 | Locale `cs` (Czech) | `locale:cs` | Should | Implemented | `DiscordLocales` |
| HB-F75 | Locale `el` (Greek) | `locale:el` | Should | Implemented | `DiscordLocales` |
| HB-F76 | Locale `bg` (Bulgarian) | `locale:bg` | Should | Implemented | `DiscordLocales` |
| HB-F77 | Locale `ru` (Russian) | `locale:ru` | Should | Implemented | `DiscordLocales` |
| HB-F78 | Locale `uk` (Ukrainian) | `locale:uk` | Should | Implemented | `DiscordLocales` |
| HB-F79 | Locale `hi` (Hindi) | `locale:hi` | Should | Implemented | `DiscordLocales` |
| HB-F80 | Locale `th` (Thai) | `locale:th` | Should | Implemented | `DiscordLocales` |
| HB-F81 | Locale `zh-CN` (Chinese, China) | `locale:zh-CN` | Should | Implemented | `DiscordLocales` |
| HB-F82 | Locale `ja` (Japanese) | `locale:ja` | Should | Implemented | `DiscordLocales` |
| HB-F83 | Locale `zh-TW` (Chinese, Taiwan) | `locale:zh-TW` | Should | Implemented | `DiscordLocales` |
| HB-F84 | Locale `ko` (Korean) | `locale:ko` | Should | Implemented | `DiscordLocales` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| HB-N01 | One process-wide `SocketsHttpHandler` (HTTP/2 with 1.1 fallback, automatic gzip/deflate/brotli, 2-minute connection lifetime for DNS refresh). |
| HB-N02 | The bot token never appears in logs or exception messages; configs print through `TokenSanitizer.Mask`. |
| HB-N03 | Request serialisation uses the shared `JsonSerializerOptions` (overridable with `DiscordClientBuilder.WithJsonOptions`); snowflakes are serialised as strings, as Discord requires. |
| HB-N04 | At most 2048 requests are in flight or queued per REST client (memory guard, not a rate limit). |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| JSON options | SDK defaults (`DiscoJson`) | any `JsonSerializerOptions` | `DiscordClientBuilder.WithJsonOptions` |
| REST base URL | `https://discord.com/api/v10` | fixed | not configurable (tests inject a `DiscordRestClient`) |
| Webhook-only client timeout | `HttpClient` default | any `TimeSpan` | `DiscordWebhookClientBuilder` |

## 9. Compatibility
- Discord still routes unversioned requests to the deprecated v6 default. Moving the base URL to a
  versioned, slash-terminated URI is not breaking for callers, but it changes the payload shapes Discord
  returns if any code relied on v6 behaviour (HB-F02).
- Adding exception subtypes is non-breaking: every typed exception derives from `DiscordApiException`.

## 10. Acceptance criteria
- [ ] Every request URI starts with `/api/v10/` (HB-F02). Add a `DiscordRestClientTests` case that asserts the absolute request URI.
- [x] HTTP 401/403/404/400 map to `InvalidTokenException`, `InsufficientPermissionException`, `DiscordResourceNotFoundException` and `InvalidRequestBodyException` (`DiscordRestClientExceptionMappingTests`).
- [x] Nested `errors` objects are flattened into field paths (`DiscordErrorParserTests`).
- [x] Snowflakes round-trip as JSON strings (`SnowflakeConverterTests`).
- [x] CDN URLs choose `gif` for animated hashes (`DiscordImageUrlTests`).
- [ ] The default avatar index uses `(user_id >> 22) % 6` for migrated usernames (HB-F37).

## 11. Open questions
- Should the REST User-Agent reuse `DiscordClientConfig.GatewayUserAgent`? Provisional: yes, a single
  `DiscordBot (url, version)` string for both transports.
- Should CDN helpers take a size and format (`?size=`, `.webp`)? Provisional: add optional `size` and
  `format` parameters to `DiscordImageUrl` when the remaining CDN endpoints are added.
