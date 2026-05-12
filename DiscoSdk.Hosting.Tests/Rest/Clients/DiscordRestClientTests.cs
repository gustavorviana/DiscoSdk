using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class DiscordRestClientTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly JsonSerializerOptions _jsonOptions = new();

    private DiscordRestClient NewClient() =>
        new("test-token", new Uri("https://discord.local/api/v10/"), _jsonOptions, _logger);

    // ---------- CreateRequestWithBody (multipart fix regression) ----------

    [Fact]
    public void CreateRequestWithBody_WithNullBody_DoesNotSetContent()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels");

        // Act
        var req = client.CreateRequestWithBody(route, HttpMethod.Get, body: null);

        // Assert
        Assert.Null(req.Content);
        Assert.Equal(HttpMethod.Get, req.Method);
    }

    /// <summary>
    /// Verifies the JSON body path produces UTF-8 <see cref="ByteArrayContent"/> with the
    /// correct content-type (no UTF-16 string + re-encoding pass via <see cref="StringContent"/>).
    /// </summary>
    [Fact]
    public async Task CreateRequestWithBody_WithPlainObject_ProducesUtf8ByteArrayContentAsync()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels");
        var body = new { name = "general" };

        // Act
        var req = client.CreateRequestWithBody(route, HttpMethod.Post, body);

        // Assert
        Assert.NotNull(req.Content);
        Assert.IsType<ByteArrayContent>(req.Content);
        Assert.Equal("application/json", req.Content!.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", req.Content.Headers.ContentType?.CharSet);

        var bytes = await req.Content.ReadAsByteArrayAsync();
        Assert.Equal(Encoding.UTF8.GetBytes("""{"name":"general"}"""), bytes);
    }

    /// <summary>
    /// Regression test for the multipart retry-safety fix: the body factory must be invoked
    /// to produce a fresh <see cref="HttpContent"/> for every request. Multipart streams are
    /// not re-readable, so retries on 429 require rebuilding the content.
    /// </summary>
    [Fact]
    public async Task CreateRequestWithBody_WithHttpContentFactory_InvokesFactoryAndAssignsResultAsync()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels");

        var calls = 0;
        Func<HttpContent> factory = () =>
        {
            calls++;
            return new StringContent("payload-" + calls, Encoding.UTF8, "application/json");
        };

        // Act
        var first = client.CreateRequestWithBody(route, HttpMethod.Post, factory);
        var second = client.CreateRequestWithBody(route, HttpMethod.Post, factory);

        // Assert — factory invoked once per call (retry-safe).
        Assert.Equal(2, calls);
        Assert.NotSame(first.Content, second.Content);
        Assert.Equal("payload-1", await first.Content!.ReadAsStringAsync());
        Assert.Equal("payload-2", await second.Content!.ReadAsStringAsync());
    }

    // ---------- Two-level bucket-hash mapping ----------

    [Fact]
    public void GetOrCreateBucket_ForSameRouteAndMethod_ReturnsSameQueue()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels/{channel_id}/messages", new Snowflake(123));

        // Act
        var first = client.GetOrCreateBucket(route, HttpMethod.Get);
        var second = client.GetOrCreateBucket(route, HttpMethod.Get);

        // Assert
        Assert.Same(first, second);
    }

    [Fact]
    public void GetOrCreateBucket_DifferentMethodsOnSameMajor_ReturnDistinctQueuesUntilHashLearned()
    {
        // Arrange — Discord may put GET and PATCH in the same hash, but we don't know yet.
        using var client = NewClient();
        var route = new DiscordRoute("channels/{channel_id}/messages", new Snowflake(123));

        // Act
        var getQueue = client.GetOrCreateBucket(route, HttpMethod.Get);
        var patchQueue = client.GetOrCreateBucket(route, HttpMethod.Patch);

        // Assert
        Assert.NotSame(getQueue, patchQueue);
    }

    /// <summary>
    /// Regression test for the bucket-hash mapping: once a hash is learned for a route, the
    /// existing route-keyed queue is migrated to the hash slot so subsequent lookups for the
    /// same route still return the same instance.
    /// </summary>
    [Fact]
    public void OnBucketHashLearned_SameRoute_PreservesQueueInstance()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels/{channel_id}/messages", new Snowflake(123));
        var initial = client.GetOrCreateBucket(route, HttpMethod.Get);

        // Act
        client.OnBucketHashLearned("GET /channels/123", "/channels/123", "abc-hash");
        var afterLearn = client.GetOrCreateBucket(route, HttpMethod.Get);

        // Assert — the instance was migrated, not recreated.
        Assert.Same(initial, afterLearn);
    }

    /// <summary>
    /// Verifies the convergence behavior: two distinct routes (different verbs on the same
    /// channel) that learn the same bucket-hash end up sharing the same queue, eliminating
    /// the duplicate rate-limit window that caused preventable 429s before this fix.
    /// </summary>
    [Fact]
    public void OnBucketHashLearned_TwoRoutesSharingHash_ConvergeOnSameQueue()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels/{channel_id}/messages", new Snowflake(123));

        var getQueue = client.GetOrCreateBucket(route, HttpMethod.Get);
        var patchQueue = client.GetOrCreateBucket(route, HttpMethod.Patch);
        Assert.NotSame(getQueue, patchQueue);

        // Act — both routes learn the same hash on the same major.
        client.OnBucketHashLearned("GET /channels/123", "/channels/123", "shared-hash");
        client.OnBucketHashLearned("PATCH /channels/123", "/channels/123", "shared-hash");

        // Assert — both methods now resolve to a single queue.
        var resolvedGet = client.GetOrCreateBucket(route, HttpMethod.Get);
        var resolvedPatch = client.GetOrCreateBucket(route, HttpMethod.Patch);
        Assert.Same(resolvedGet, resolvedPatch);

        // The convergent queue is the one created by the first migration (GET).
        Assert.Same(getQueue, resolvedGet);
    }

    // ---------- Idle bucket eviction ----------

    /// <summary>
    /// Verifies that the eviction sweep removes bucket queues that have been idle longer
    /// than the supplied threshold, along with any matching route-to-hash mappings — closing
    /// the unbounded-growth memory leak that long-running bots would otherwise accumulate.
    /// </summary>
    [Fact]
    public async Task EvictIdleBuckets_RemovesIdleQueuesAndRouteMappingsAsync()
    {
        // Arrange — create a queue and learn a hash so both dictionaries have entries.
        using var client = NewClient();
        var route = new DiscordRoute("channels/{channel_id}/messages", new Snowflake(123));

        client.GetOrCreateBucket(route, HttpMethod.Get);
        client.OnBucketHashLearned("GET /channels/123", "/channels/123", "abc-hash");
        Assert.Equal(1, client.BucketCount);
        Assert.Equal(1, client.RouteToHashCount);

        // Wait long enough that the queue is idle past the threshold.
        await Task.Delay(50);

        // Act
        var evicted = client.EvictIdleBuckets(TimeSpan.FromMilliseconds(10));

        // Assert
        Assert.Equal(1, evicted);
        Assert.Equal(0, client.BucketCount);
        Assert.Equal(0, client.RouteToHashCount);
    }

    [Fact]
    public async Task EvictIdleBuckets_KeepsRecentlyUsedQueuesAsync()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels/{channel_id}", new Snowflake(456));
        client.GetOrCreateBucket(route, HttpMethod.Get);
        Assert.Equal(1, client.BucketCount);

        // Brief delay; the queue is still well within the idle threshold.
        await Task.Delay(10);

        // Act — threshold an order of magnitude larger than the elapsed delay.
        var evicted = client.EvictIdleBuckets(TimeSpan.FromSeconds(30));

        // Assert — queue is preserved.
        Assert.Equal(0, evicted);
        Assert.Equal(1, client.BucketCount);
    }

    [Fact]
    public void OnBucketHashLearned_Idempotent_DoesNotChangeQueueOnDuplicateCall()
    {
        // Arrange
        using var client = NewClient();
        var route = new DiscordRoute("channels/{channel_id}/messages", new Snowflake(123));
        client.GetOrCreateBucket(route, HttpMethod.Get);

        // Act — call twice with same arguments
        client.OnBucketHashLearned("GET /channels/123", "/channels/123", "abc-hash");
        var afterFirst = client.GetOrCreateBucket(route, HttpMethod.Get);

        client.OnBucketHashLearned("GET /channels/123", "/channels/123", "abc-hash");
        var afterSecond = client.GetOrCreateBucket(route, HttpMethod.Get);

        // Assert
        Assert.Same(afterFirst, afterSecond);
    }
}
