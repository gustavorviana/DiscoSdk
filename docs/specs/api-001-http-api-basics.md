# SPEC-API-001 — HTTP API basics

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-001](../prd/api-001-http-api-basics.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [reference](https://docs.discord.com/developers/reference), [topics/opcodes-and-status-codes](https://docs.discord.com/developers/topics/opcodes-and-status-codes) |
| **Last updated** | 2026-09-29 |

## 1. Summary
All REST calls go through one `DiscordRestClient` per `DiscordClient`. It owns a shared
`SocketsHttpHandler`, sets the `Bot` authorization header and User-Agent once, serialises bodies with
the SDK's `JsonSerializerOptions`, routes every request through a per-bucket queue (SPEC-API-002), and
converts non-success responses into a typed `DiscordApiException` hierarchy parsed from Discord's error
JSON. Domain clients (`ChannelClient`, `GuildClient`, …) only build a `DiscordRoute` and pick an
`HttpMethod`. Value types in `DiscoSdk` (`Snowflake`, `DiscordImageBuffer`, `DiscordImageUrl`,
`Mention`, `MessageTextBuilder`) cover IDs, uploads, CDN URLs and message markup.

## 2. Projects and dependencies
- `DiscoSdk`: `Snowflake`, `DiscordRoute`, `DiscordApiError`, `DiscordValidationError`, exceptions,
  `DiscordImageBuffer`, `DiscordImageUrl`, `DiscordLocales`, `Mention`, `MessageTextBuilder`,
  `TimestampFormat`, JSON converters (`DiscoJson`, `SnowflakeConverter`).
- `DiscoSdk.Hosting`: `DiscordRestClient`, `ClientUtils` (multipart helpers), `DiscordErrorParser`,
  `TransientRetryPolicy` (Polly.Core).
- Packages: `Polly.Core`, `Microsoft.Extensions.Logging.Abstractions`.

## 3. Models and contracts
- `DiscordRoute(template, args)`: a template such as `channels/{channel_id}/messages` plus positional
  arguments. `ToString()` substitutes them; `GetBucketPath()` derives the major-parameter scope.
- `DiscordApiError` (`Code`, `Message`, `ValidationErrors`) is parsed from `{ code, message, errors }`.
  Nested `errors` objects are flattened into `DiscordValidationError(path, code, message)`.
- `Snowflake` is a `readonly struct` over `ulong`, with `CreatedAt` (Discord epoch 2015-01-01),
  `WorkerId`, `ProcessId` and `Increment`. It is serialised as a JSON string by `SnowflakeConverter`.
- `DiscordImageBuffer(byte[], extension)` detects the image type from the magic bytes and exposes `ToBase64()`. Each action builds the `data:<type>;base64,…` URI itself (`EditGuildAction`, `RoleAction`). `DiscordSoundBuffer.ToDataUri()` does the same for audio.
- `DiscordImageUrl(url, extension)` has static `Parse*` factories for avatars, banners, icons, splashes
  and discovery splashes; a hash starting with `a_` selects `gif`, otherwise `png`.

## 4. Components
| Project | Type | Responsibility |
|---|---|---|
| `DiscoSdk` | `IDiscordRestClient` | `SendAsync<T>(DiscordRoute, HttpMethod, body, ct)`, `SendWithReasonAsync`, auth override overloads. |
| `DiscoSdk.Hosting` | `DiscordRestClient` | Transport, auth, User-Agent, in-flight gate (2048), bucket queues, error translation, idle-bucket eviction every 5 minutes (15-minute idle threshold). |
| `DiscoSdk.Hosting` | `ClientUtils.SendMultipartAsync` / `SendFormDataAsync` | Build `multipart/form-data` with `payload_json` + `files[n]` (messages) or flat fields (stickers, soundboard) through a `Func<HttpContent>` so retries rebuild single-use streams. |
| `DiscoSdk.Hosting` | `DiscordErrorParser` | JSON error body → `DiscordApiError`. |
| `DiscoSdk.Hosting` | `TransientRetryPolicy` | 3 retries with 200 ms exponential backoff plus a circuit breaker (50% failures over 30 s, minimum 10 calls, 5 s break) for 5xx and transport errors. |
| `DiscoSdk` | `Mention`, `MentionBuilderBase`, `MessageTextBuilder` | Markup for users, roles and timestamps; tracks mentions to generate `AllowedMentions`. |

## 5. Public API
Bot authors never touch `DiscordRestClient` directly. They see:
- exceptions: `DiscordApiException` (`StatusCode`, `DiscordCode`, `ValidationErrors`) and its subtypes;
- value types: `Snowflake`, `DiscordImageBuffer`, `DiscordImageUrl` (through `IUser.Avatar`,
  `IGuild.Icon`, …) and `DiscordLocales`;
- builders: `MessageTextBuilder`, `Mention`.

```csharp
var icon = DiscordImageBuffer.LoadFile("icon.png");
await guild.Edit().SetIcon(icon).ExecuteAsync();
```

## 6. Discord surface
- Base URL: `https://discord.com/api/v10/` (`DiscordRestClient.DefaultApiUri`, trailing slash enforced).
- Headers: `Authorization: Bot <token>`, `User-Agent: DiscordBot (https://github.com/gustavorviana/DiscoSdk, <version>)`,
  `Content-Type: application/json; charset=utf-8` or multipart, and `X-Audit-Log-Reason`
  (SPEC-API-012).
- CDN: `https://cdn.discordapp.com/` (avatars, banners, icons, splashes, discovery splashes, member avatars).

## 7. Flows
**Request**
1. A domain client builds a `DiscordRoute` and calls `SendAsync`.
2. `DispatchAsync` waits on the in-flight gate, then `GetOrCreateBucket(route, method)` enqueues the request factory.
3. The bucket queue applies the global and per-bucket limits (SPEC-API-002), creates the `HttpRequestMessage` from the factory and sends it through `TransientRetryPolicy`.
4. On 2xx, the body is deserialised from the stream (204 returns `default`). Anything else goes to `GetDiscordExceptionAsync`.

**Error**
1. The body is read as a string and handed to `DiscordErrorParser.Parse`; parse failures are swallowed.
2. `BuildException(status, reason, error)` picks the subtype: 401 with 40001/50014 → `InvalidTokenException`; 403 with 50001/50013 → `InsufficientPermissionException`; 404 with 10001–10068 → `DiscordResourceNotFoundException`; 400 with 50035 → `InvalidRequestBodyException`; anything else → `DiscordApiException`.

## 8. Concurrency and lifecycle
- `DiscordRestClient` is thread-safe. Buckets live in a `ConcurrentDictionary`, and requests in the same bucket are serialised.
- The `HttpClient` shares a static handler that is never disposed. Disposing the client cancels the eviction loop and in-flight waits.
- Every public call accepts a `CancellationToken`. A cancelled request releases its in-flight slot.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Base URI given without trailing slash (`…/api/v10`) and relative route (`channels/1`) | `DiscordRestClient` appends the slash, so the request goes to `…/api/v10/channels/1` (HB-F02). |
| Error body is not JSON (Cloudflare HTML, empty body) | `DiscordApiException` with the HTTP status and reason phrase, `DiscordCode == null`. |
| 2xx with a `null` JSON body where a value is expected | `DiscordApiException("Discord API returned empty JSON.")`. |
| Audit-log reason longer than 512 characters | Truncated to 512, then URL-encoded. |
| Multipart request retried after a 5xx | The content factory rebuilds the streams for each attempt. |
| `Mention.FromChannel(id)` appended to text | Renders `<#id>` and is not added to allowed mentions (HB-F19). |
| User without an avatar on the new username system | **Current:** `embed/avatars/{id % 5}` or `{discriminator % 5}`. **Expected:** `(id >> 22) % 6` (HB-F37). |

## 10. Observability
- REST metrics and logs (bucket waits, 429s, invalid-request counter) are covered in SPEC-API-002 and
  SPEC-SDK-05.
- The token never appears in logs. `DiscordClientConfig.ToString()` masks it via `TokenSanitizer.Mask`.

## 11. Tests
| Test class | Covers |
|---|---|
| `DiscordRestClientTests` | HB-F01, HB-F04, HB-F07, HB-N04 |
| `DiscordRestClientExceptionMappingTests` | HB-F05, HB-F07 |
| `DiscordErrorParserTests` | HB-F06 |
| `TransientRetryPolicyTests` | HB-F07 |
| `DiscordRouteTests` | HB-F02 (template substitution only, not the base URI) |
| `SnowflakeConverterTests` | HB-F09, HB-F11 |
| `DiscoJsonTests`, `ColorConverterTests` | HB-F12, HB-N03 |
| `DiscordImageBufferTests`, `DiscordSoundBufferTests` | HB-F14 |
| `DiscordImageUrlTests` | HB-F32 – HB-F38 |
| `MessageTextBuilderTests`, `MentionBuilderTests` | HB-F18 – HB-F29 |
| `MessageClientTests`, `StickerClientTests`, `SoundboardSoundClientTests` | HB-F15 |
| `SlashCommandLocalizerTests` | HB-F53 – HB-F84 (locale validation) |

`DiscordRestClientTests` asserts the resolved request path and the REST User-Agent (HB-F02, HB-F03); `MessageTextBuilderTests` covers channel mention rendering (HB-F19); `DiscordLocalesTests` covers `es-419` (HB-F59).

Implemented or Partial requirements without a covering test: HB-F08, HB-F13, HB-F16, HB-F39, HB-N01 – HB-N02.

## 12. History
| Commit | Change |
|---|---|
| `42add64` | Typed subclasses for token, not-found and validation errors. |
| `124b81b` | 403 permission errors → `InsufficientPermissionException`. |
| `2b068c1` | Cloudflare invalid-request guard, token masking, shared-scope 429 logging. |
| `15c4af6` | `X-Audit-Log-Reason` on every auditable mutation. |
| `70f78e8` | `IMessage.ToBuilder` full-fidelity fork (attachment editing). |

Next steps, each a single commit:
1. Add `s`/`S` to `TimestampFormat`.
2. Fix the default-avatar index and add CDN helpers for emoji, sticker, role icon and application icon.

## 13. Decisions and rejected alternatives
- **Static shared handler** rather than an `HttpClient` per client: this pools connections across shards and clients and refreshes DNS through `PooledConnectionLifetime`. A caller-supplied `HttpMessageHandler` was rejected to keep the rate-limit invariants inside the SDK.
- **Typed exception subclasses** rather than a `Result<T>`: existing `catch (DiscordApiException)` code keeps working, and callers can pattern-match on the subtype.
- **Stream deserialisation** rather than `ReadAsStringAsync`: it avoids one full-body allocation per response.
