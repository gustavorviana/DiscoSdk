using DiscoSdk.Hosting.Observability;
using DiscoSdk.Hosting.Rest.RateLimit;
using DiscoSdk.Hosting.Tests.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace DiscoSdk.Hosting.Tests.Observability;

/// <summary>
/// Verifies that <see cref="BucketRequestQueue"/> publishes the Phase 1 REST instruments —
/// <c>discosdk.rest.requests</c>, <c>discosdk.rest.latency</c>, and
/// <c>discosdk.rest.rate_limited</c> — with the expected tags. Uses
/// <see cref="MeterListenerCapture"/> to assert without going through OTel.
/// </summary>
[Collection("Observability")]
public class BucketRequestQueueMetricsTests
{
    // Unique per-class bucket name keeps MeterListenerCapture readings from colliding with
    // parallel non-observability tests that exercise other BucketRequestQueue instances.
    private const string Bucket = "obs-rest-metric-bucket";
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly TimeProvider _timeProvider = TimeProvider.System;

    private static HttpResponseMessage MakeResponse(HttpStatusCode code, double? resetAfter = null, string? scope = null)
    {
        var response = new HttpResponseMessage(code) { Content = new StringContent("{}") };
        if (resetAfter.HasValue)
            response.Headers.Add("X-RateLimit-Reset-After", resetAfter.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (scope is not null)
            response.Headers.Add("X-RateLimit-Scope", scope);
        return response;
    }

    private BucketRequestQueue NewQueue(HttpClient http) =>
        new(new GlobalRateLimitManager(_logger, _timeProvider),
            new InvalidRequestTracker(_timeProvider, _logger),
            _logger, http, Bucket,
            shutdownToken: default,
            _timeProvider, onHashLearned: null);

    private IEnumerable<CapturedMeasurement<long>> RequestsFor(MeterListenerCapture capture)
        => capture.LongFor("discosdk.rest.requests").Where(m => (string?)m.Tag(DiagnosticTags.Route) == Bucket);

    private IEnumerable<CapturedMeasurement<double>> LatencyFor(MeterListenerCapture capture)
        => capture.DoubleFor("discosdk.rest.latency").Where(m => (string?)m.Tag(DiagnosticTags.Route) == Bucket);

    private IEnumerable<CapturedMeasurement<long>> RateLimitedFor(MeterListenerCapture capture)
        => capture.LongFor("discosdk.rest.rate_limited").Where(m => (string?)m.Tag(DiagnosticTags.Route) == Bucket);

    [Fact]
    public async Task SuccessfulRequest_PublishesRequestsCounterWith2xxClassAsync()
    {
        var handler = new StubHttpMessageHandler(_ => MakeResponse(HttpStatusCode.OK));
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);
        using var capture = new MeterListenerCapture("discosdk.rest.requests", "discosdk.rest.latency");

        await queue.ExecuteAsync(() => new HttpRequestMessage(HttpMethod.Get, "https://discord.local/test"), CancellationToken.None);

        var counter = Assert.Single(RequestsFor(capture));
        Assert.Equal(1, counter.Value);
        Assert.Equal("GET", counter.Tag(DiagnosticTags.HttpMethod));
        Assert.Equal("2xx", counter.Tag(DiagnosticTags.HttpStatusClass));

        var latency = Assert.Single(LatencyFor(capture));
        Assert.True(latency.Value >= 0d);
        Assert.Equal("GET", latency.Tag(DiagnosticTags.HttpMethod));
    }

    [Fact]
    public async Task ServerError_PublishesRequestsCounterWith5xxClassAsync()
    {
        var handler = new StubHttpMessageHandler(_ => MakeResponse(HttpStatusCode.InternalServerError));
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);
        using var capture = new MeterListenerCapture("discosdk.rest.requests");

        await queue.ExecuteAsync(() => new HttpRequestMessage(HttpMethod.Post, "https://discord.local/test"), CancellationToken.None);

        // TransientRetryPolicy retries on 5xx — every attempt records.
        var counters = RequestsFor(capture).ToList();
        Assert.NotEmpty(counters);
        Assert.All(counters, m =>
        {
            Assert.Equal("POST", m.Tag(DiagnosticTags.HttpMethod));
            Assert.Equal("5xx", m.Tag(DiagnosticTags.HttpStatusClass));
        });
    }

    [Fact]
    public async Task RateLimited_PublishesRateLimitedCounterWithScopeAsync()
    {
        var first429 = true;
        var handler = new StubHttpMessageHandler(_ =>
        {
            if (first429)
            {
                first429 = false;
                return MakeResponse(HttpStatusCode.TooManyRequests, resetAfter: 0.001, scope: "shared");
            }
            return MakeResponse(HttpStatusCode.OK);
        });
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);
        using var capture = new MeterListenerCapture("discosdk.rest.rate_limited");

        await queue.ExecuteAsync(() => new HttpRequestMessage(HttpMethod.Get, "https://discord.local/test"), CancellationToken.None);

        var rl = Assert.Single(RateLimitedFor(capture));
        Assert.Equal(1, rl.Value);
        Assert.Equal("shared", rl.Tag(DiagnosticTags.Scope));
    }

    [Fact]
    public async Task RateLimited_MissingScopeHeader_DefaultsToUserAsync()
    {
        var first429 = true;
        var handler = new StubHttpMessageHandler(_ =>
        {
            if (first429)
            {
                first429 = false;
                return MakeResponse(HttpStatusCode.TooManyRequests, resetAfter: 0.001);
            }
            return MakeResponse(HttpStatusCode.OK);
        });
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);
        using var capture = new MeterListenerCapture("discosdk.rest.rate_limited");

        await queue.ExecuteAsync(() => new HttpRequestMessage(HttpMethod.Get, "https://discord.local/test"), CancellationToken.None);

        var rl = Assert.Single(RateLimitedFor(capture));
        Assert.Equal("user", rl.Tag(DiagnosticTags.Scope));
    }
}
