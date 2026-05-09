namespace DiscoSdk.Hosting.Rest.RateLimit;

/// <summary>
/// Strongly-typed view of Discord's rate-limit response headers. Modeled as a record struct
/// because every Discord response builds one — keeping it on the stack avoids per-request
/// heap allocations at fleet scale.
/// </summary>
internal readonly record struct DiscordRateLimitHeader(
    string? Bucket,
    int? Limit,
    int? Remaining,
    double? ResetAfter,
    string? Scope);
