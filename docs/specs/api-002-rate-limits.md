# SPEC-API-002 — Rate limits

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-002](../prd/api-002-rate-limits.md) |
| **Projects** | `DiscoSdk.Hosting` |
| **Discord docs** | [topics/rate-limits](https://docs.discord.com/developers/topics/rate-limits) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`DiscordRestClient` keeps a dictionary of `BucketRequestQueue` instances. A request is first keyed by
`"{METHOD} {bucket path}"`, where `DiscordRoute.GetBucketPath()` cuts the template at the first major
parameter. Once a response reveals `X-RateLimit-Bucket`, the route key is remapped to a
`hash + major id` key. Each queue serialises its requests, sleeps for `Reset-After` when the bucket is
exhausted, consults the shared `GlobalRateLimitManager`, and retries 429s up to 5 times.
`InvalidRequestTracker` counts 401/403/429 responses in a 10-minute fixed window and pauses sends at 9,500.

## 2. Projects and dependencies
`DiscoSdk.Hosting/Rest/RateLimit/*` and `DiscordRestClient`, with `TimeProvider` for testability. There
are no external packages beyond logging.

## 3. Models and contracts
- `DiscordRateLimitHeader(Bucket, Limit, Remaining, ResetAfter, Scope)`: a `readonly record struct`
  parsed from each response, with no heap allocation.
- The 429 body carries `{ message, retry_after, global, code? }`. `retry_after` is read only when the
  headers are missing.

## 4. Components
| Type | Responsibility |
|---|---|
| `DiscordRestClient.GetOrCreateBucket(route, method)` | Resolves the route key to the hash key, then gets or creates a `BucketRequestQueue`. |
| `DiscordRestClient.EvictionLoopAsync` | Every 5 minutes, disposes queues idle for 15 minutes or more. |
| `BucketRequestQueue.ExecuteAsync(factory, ct)` | Gate (1 permit), then the invalid-request pause, the global wait, the send and the header parse. On a 429 it retries; otherwise it returns. |
| `GlobalRateLimitManager` | Holds a single "blocked until" deadline. Callers coordinate through one semaphore to avoid a thundering herd. |
| `InvalidRequestTracker` | Fixed-window counter, `CloudflareInvalidLimit = 10_000`, `SafetyThreshold = 9_500`, `Window = 10 min`. |

## 5. Public API
None. The behaviour is implicit for every `IRestAction`. The only knob is `DiscordClientBuilder.WithTimeProvider`.

## 6. Discord surface
- Response headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset-After`,
  `X-RateLimit-Bucket`, `X-RateLimit-Global`, `X-RateLimit-Scope`, and `Retry-After` from Cloudflare.
- Major parameters: `{channel_id}`, `{guild_id}`, `{webhook_id}`, `{webhook_token}`, `{interaction_id}`,
  `{interaction_token}`, plus a fallback to `{application_id}` when none of these is present.

## 7. Flows
1. `ExecuteAsync` acquires the bucket gate.
2. `InvalidRequestTracker.WaitIfNearLimitAsync`.
3. `GlobalRateLimitManager.WaitForGlobalAsync`.
4. If the previous response left `Remaining == 0`, wait `ResetAfter`.
5. Send the request (the transient retry policy sits inside the send).
6. Record metrics and the invalid-request count, then parse the headers and remap to the hash key if a new bucket appeared.
7. Handle a 429:
   - global → `ReadAndWaitForGlobalAsync`, then retry;
   - shared → log it as shared, then back off;
   - user → back off by `Reset-After`, falling back to `Retry-After`, then to 1 s.
   After 5 attempts, return the 429 response and let SPEC-API-001 turn it into a `DiscordApiException`.
8. Release the gate.

## 8. Concurrency and lifecycle
- Order is FIFO within one bucket (semaphore fairness). Different buckets run in parallel, up to the global in-flight gate of 2048.
- The linked cancellation sources are: the caller's token, the bucket's own source (cancelled on eviction) and the client shutdown token.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| 429 with no rate-limit headers (Cloudflare) | Waits `Retry-After`, or 1 s if absent, then retries. |
| 5 consecutive 429s | Raises `DiscordApiException` with status 429. |
| Two routes discovered to share one bucket hash | Both route keys map to the same queue after the first responses. |
| Bucket evicted while a request waits | That request is cancelled with `OperationCanceledException`. |
| Shared-scope 429 | **Current:** counts toward the invalid-request budget. **Expected:** excluded (RL-F07). |

## 10. Observability
- Logs: `Warning` "Global rate limit encountered. Retrying after {RetryAfterSeconds} seconds."; shared-scope 429 logs; an invalid-request threshold warning.
- Metrics (`DiscoSdkDiagnostics`): REST request duration and count, bucket wait, 429s tagged by `scope`. See SPEC-SDK-05.

## 11. Tests
| Test class | Covers |
|---|---|
| `BucketRequestQueueTests` | RL-F01, RL-F02, RL-F03, RL-F04, RL-N01, RL-N03 |
| `GlobalRateLimitManagerTests` | RL-F05 |
| `BucketRequestQueueMetricsTests` | RL-N05 |
| `DiscordRouteTests` | RL-F01 (bucket path from major parameters) |
| `DiscordRestClientTests` | RL-N02, RL-F08 |

## 12. History
| Commit | Change |
|---|---|
| `2b068c1` | Cloudflare invalid-request guard, token masking, shared-scope 429 logging. |
| `5ecbc74` | `Meter "DiscoSdk"` with REST instruments. |

Next steps:
1. Skip shared-scope 429s in `InvalidRequestTracker.RecordResponse`.
2. Add a proactive global token bucket that bypasses `interactions/{id}/{token}/callback` and webhook-token routes.
3. Mark webhooks as dead after a 404.

## 13. Decisions and rejected alternatives
- **Calling task does the work** rather than a worker loop per bucket: fewer allocations, and exceptions surface on the caller's stack.
- **`Reset-After` rather than `Reset`**: this makes waits immune to host clock skew.
- **Fixed window for invalid requests**: it mirrors how Cloudflare measures, and a sliding window would drift from Cloudflare's count.
