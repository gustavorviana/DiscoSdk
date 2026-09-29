# PRD-API-002 — Rate limits

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-002](../specs/api-002-rate-limits.md) |
| **Projects** | `DiscoSdk.Hosting` |
| **Discord docs** | [topics/rate-limits](https://docs.discord.com/developers/topics/rate-limits) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Discord limits REST traffic per route bucket, per bot (global, 50 req/s) and per IP (10,000 invalid
responses per 10 minutes, enforced by Cloudflare with a one-hour ban). A bot that ignores the headers gets
429 storms. A bot that retries blindly can get every process behind the same IP banned.

## 2. Goals
- Bot authors never see a 429. The SDK waits and retries transparently, within a bounded number of attempts.
- Requests that share a bucket are serialised, so each request sees the headers of the previous one.
- The SDK never trips the Cloudflare invalid-request ban on its own.
- Rate-limit behaviour is observable through metrics and logs.

## 3. Out of scope
- The gateway send limit (120 commands per 60 s) is covered in [PRD-API-003](api-003-gateway-connection.md).
- Retries of transient 5xx and transport errors are covered in [PRD-API-001](api-001-http-api-basics.md), via `TransientRetryPolicy`.

## 4. Usage scenarios
- As a bot author, when I bulk-assign roles to 500 members, calls in the same bucket queue up and finish
  without errors, only slower.
- When another process on the same IP floods 403s, my bot pauses before reaching the Cloudflare cap
  instead of being banned.
- When I watch Grafana, I see bucket wait time and 429s by scope.

## 5. Desired developer experience

```csharp
// Nothing to configure: every IRestAction goes through the bucket queues.
foreach (var member in members)
    await guild.Members.AddRole(member.Id, roleId).ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| RL-F01 | Per-route buckets keyed by `X-RateLimit-Bucket` plus the major parameter (`channel_id`, `guild_id`, `webhook_id`/token, `interaction_id`/token) | — | Must | Implemented | `DiscordRestClient.GetOrCreateBucket`, `DiscordRoute.GetBucketPath` (route-keyed queue migrated to the hash key once Discord reveals it) |
| RL-F02 | Honour `X-RateLimit-Remaining` and `X-RateLimit-Reset-After` before the next request | — | Must | Implemented | `BucketRequestQueue` (one request at a time per bucket; waits on the local clock using `Reset-After`, not the absolute `Reset`) |
| RL-F03 | Parse `X-RateLimit-Limit`, `X-RateLimit-Bucket`, `X-RateLimit-Scope` | — | Must | Implemented | `BucketRequestQueue` → `DiscordRateLimitHeader` |
| RL-F04 | Recover from 429 using `X-RateLimit-Reset-After`, `retry_after` or `Retry-After` | — | Must | Implemented | `BucketRequestQueue` (up to 5 attempts; 1 s fallback when no header is present) |
| RL-F05 | Global 429 (`X-RateLimit-Global`) blocks every request until `retry_after` elapses | — | Must | Implemented | `GlobalRateLimitManager.ReadAndWaitForGlobalAsync` / `WaitForGlobalAsync` |
| RL-F06 | Stay under the global limit of 50 requests per second | — | Should | Partial | Reactive only: the SDK waits after a global 429 but does not pace requests ahead of time. Interaction endpoints, which are exempt, share the same pipeline. |
| RL-F07 | `X-RateLimit-Scope: shared` 429s are not counted as invalid requests | — | Should | Partial | Shared-scope 429s are logged separately (commit `2b068c1`), but `InvalidRequestTracker.RecordResponse` counts every 401/403/429, including shared ones (conservative). |
| RL-F08 | Invalid-request budget (10,000 per 10 min per IP): pause before the Cloudflare ban | — | Must | Implemented | `InvalidRequestTracker` (fixed window, pauses at 9,500) |
| RL-F09 | Stop reusing a webhook after a 404 | — | Should | Missing | 404s raise `DiscordResourceNotFoundException`; nothing marks the webhook as dead. |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| RL-N01 | No background worker per bucket: the calling task does the work under a 1-permit `SemaphoreSlim`. |
| RL-N02 | Idle buckets (15 minutes without use) are evicted by a sweeper every 5 minutes, so memory stays bounded. |
| RL-N03 | Rate-limit waits honour the caller's `CancellationToken`, and disposing the client cancels every wait. |
| RL-N04 | Clock skew between host and Discord does not affect waits (relative `Reset-After` only). |
| RL-N05 | Rate-limit waits, 429s by scope and invalid-request counts are exported on `Meter "DiscoSdk"` (see PRD-SDK-05). |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|
| 429 retry attempts per request | 5 | fixed | — |
| Invalid-request safety threshold | 9,500 / 10 min | fixed | — |
| Time source | `TimeProvider.System` | any `TimeProvider` | `DiscordClientBuilder.WithTimeProvider` |

## 9. Compatibility
No public surface. Changing limits or adding proactive pacing is not a breaking change.

## 10. Acceptance criteria
- [x] Requests in one bucket are sent one at a time and wait for `Reset-After` when `Remaining == 0` (`BucketRequestQueueTests`).
- [x] A global 429 delays every bucket (`GlobalRateLimitManagerTests`).
- [x] Metrics record bucket waits and 429 scope (`BucketRequestQueueMetricsTests`).
- [ ] Shared-scope 429s do not advance the invalid-request counter (RL-F07).
- [ ] A proactive 50 req/s limiter exists, excluding interaction callbacks (RL-F06).

## 11. Open questions
- Should proactive global pacing be opt-in? Provisional: enabled by default at 50 req/s, and configurable for
  bots with a raised limit.
- Should the invalid-request tracker be shared process-wide, since the limit is per IP? Provisional: yes,
  make it static like the HTTP handler.
