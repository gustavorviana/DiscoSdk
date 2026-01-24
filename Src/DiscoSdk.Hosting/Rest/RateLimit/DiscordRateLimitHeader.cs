namespace DiscoSdk.Hosting.Rest.RateLimit;

internal class DiscordRateLimitHeader
{
    public string? Bucket { get; set; }
    public int? Limit { get; set; }
    public int? Remaining { get; set; }
    public double? ResetAfter { get; set; }
    public string? Scope { get; set; }
    public DateTimeOffset ResetAt { get; set; }

    public override string ToString()
    {
        return $"BucketHash={Bucket}, Limit={Limit}, Remaining={Remaining}, Scope={Scope}";
    }
}