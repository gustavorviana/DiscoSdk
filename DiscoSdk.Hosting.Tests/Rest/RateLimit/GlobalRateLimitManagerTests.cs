using DiscoSdk.Hosting.Rest.RateLimit;
using DiscoSdk.Logging;
using NSubstitute;
using System.Net;
using System.Text;

namespace DiscoSdk.Hosting.Tests.Rest.RateLimit;

public class GlobalRateLimitManagerTests
{
    private readonly ILogger _logger;

    public GlobalRateLimitManagerTests()
    {
        _logger = Substitute.For<ILogger>();
    }

    private static HttpResponseMessage CreateGlobalRateLimitResponse(double retryAfterSeconds)
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.Add("X-RateLimit-Global", "true");
        var json = $@"{{""retry_after"": {retryAfterSeconds}}}";
        response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return response;
    }

    private static HttpResponseMessage Create429ResponseWithoutGlobalHeader()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var json = @"{""retry_after"": 1.0}";
        response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return response;
    }

    private static HttpResponseMessage Create429ResponseWithoutRetryAfter()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.Add("X-RateLimit-Global", "true");
        var json = @"{}";
        response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return response;
    }

    [Fact]
    public void WaitForGlobalAsync_WhenNoRateLimit_ReturnsImmediately()
    {
        // Arrange
        var manager = new GlobalRateLimitManager(_logger);

        // Act
        var task = manager.WaitForGlobalAsync();

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task ReadAndWaitForGlobalAsync_WhenNot429_ReturnsFalse_Async()
    {
        // Arrange
        var manager = new GlobalRateLimitManager(_logger);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        // Act
        var result = await manager.ReadAndWaitForGlobalAsync(response);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ReadAndWaitForGlobalAsync_When429WithoutGlobalHeader_ReturnsFalse_Async()
    {
        // Arrange
        var manager = new GlobalRateLimitManager(_logger);
        var response = Create429ResponseWithoutGlobalHeader();

        // Act
        var result = await manager.ReadAndWaitForGlobalAsync(response);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ReadAndWaitForGlobalAsync_When429WithoutRetryAfter_ReturnsFalse_Async()
    {
        // Arrange
        var manager = new GlobalRateLimitManager(_logger);
        var response = Create429ResponseWithoutRetryAfter();

        // Act
        var result = await manager.ReadAndWaitForGlobalAsync(response);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ReadAndWaitForGlobalAsync_WithZeroRetryAfter_HandlesCorrectly_Async()
    {
        // Arrange
        var manager = new GlobalRateLimitManager(_logger);
        var response = CreateGlobalRateLimitResponse(0.0);
        var startTime = DateTimeOffset.UtcNow;

        // Act
        var result = await manager.ReadAndWaitForGlobalAsync(response);

        // Assert
        Assert.True(result);
        var elapsed = DateTimeOffset.UtcNow - startTime;

        Assert.True(elapsed.TotalMilliseconds < 50, 
            $"Expected near-immediate return, got {elapsed.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task ReadAndWaitForGlobalAsync_WithNegativeRetryAfter_HandlesCorrectly_Async()
    {
        // Arrange
        var manager = new GlobalRateLimitManager(_logger);
        var response = CreateGlobalRateLimitResponse(-1.0);
        var startTime = DateTimeOffset.UtcNow;

        // Act
        var result = await manager.ReadAndWaitForGlobalAsync(response);

        // Assert
        Assert.True(result);
        var elapsed = DateTimeOffset.UtcNow - startTime;

        Assert.True(elapsed.TotalMilliseconds < 50, 
            $"Expected immediate return with negative retry_after, got {elapsed.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task ReadAndWaitForGlobalAsync_WithCancellationToken_RespectsCancellation_Async()
    {
        // Arrange
        var manager = new GlobalRateLimitManager(_logger);
        var response = CreateGlobalRateLimitResponse(1.0);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await manager.ReadAndWaitForGlobalAsync(response, cts.Token));
    }
}