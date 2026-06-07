using DiscoSdk;
using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Managers;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Managers;

/// <summary>
/// Verifies that <see cref="GuildManager"/> emits a one-shot Warning when its cache is touched
/// without the <see cref="DiscordIntent.Guilds"/> intent. Without that intent Discord never sends
/// <c>GUILD_CREATE</c>, so the cache stays empty forever and consumers silently see "no guilds".
/// </summary>
public class GuildCacheIntentGuardTests : IDisposable
{
    public GuildCacheIntentGuardTests() => GuildCacheWarnTracker.ResetForTests();
    public void Dispose() => GuildCacheWarnTracker.ResetForTests();

    private static DiscordClient NewClient(DiscordIntent intents, ILogger logger)
    {
        var http = Substitute.For<IDiscordRestClient>();
        http.JsonOptions.Returns(new JsonSerializerOptions());
        return DiscordClientBuilder.Create("test-token")
            .WithIntents(intents)
            .WithRestClient(http)
            .WithLogger(logger)
            .Build();
    }

    private static int WarnCalls(ILogger logger) =>
        logger.ReceivedCalls().Count(c =>
            c.GetMethodInfo().Name == nameof(ILogger.Log) &&
            c.GetArguments()[0] is LogLevel.Warning);

    [Fact]
    public void AllReadWithoutGuildsIntent_LogsWarningOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);

        _ = client.Guilds.All;
        _ = client.Guilds.All;
        _ = client.Guilds.All;

        Assert.Equal(1, WarnCalls(logger));
    }

    [Fact]
    public void AllReadWithGuildsIntent_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.Guilds, logger);

        _ = client.Guilds.All;

        Assert.Equal(0, WarnCalls(logger));
    }

    [Fact]
    public void TryGetCacheMissWithoutGuildsIntent_LogsWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);

        client.Guilds.TryGet(new Snowflake(123), out _);

        Assert.Equal(1, WarnCalls(logger));
    }

    [Fact]
    public void TryGetEmptySnowflake_DoesNotWarn()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);

        client.Guilds.TryGet(default, out _);

        Assert.Equal(0, WarnCalls(logger));
    }

    [Fact]
    public async Task GetAsyncWithoutGuildsIntent_DoesNotHitRestAsync()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var http = Substitute.For<IDiscordRestClient>();
        http.JsonOptions.Returns(new JsonSerializerOptions());
        var client = DiscordClientBuilder.Create("test-token")
            .WithIntents(DiscordIntent.GuildMessages)
            .WithRestClient(http)
            .WithLogger(logger)
            .Build();

        var result = await client.Guilds.GetAsync(new Snowflake(123));

        Assert.Null(result);
        // No SendAsync of any shape should have hit the REST client.
        Assert.Empty(http.ReceivedCalls().Where(c => c.GetMethodInfo().Name.StartsWith("Send")));
    }

    [Fact]
    public void AllWithoutGuildsIntent_ReturnsEmpty()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);

        Assert.Empty(client.Guilds.All);
    }

    [Fact]
    public void TryGetWithoutGuildsIntent_ReturnsFalseAndNull()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);

        Assert.False(client.Guilds.TryGet(new Snowflake(123), out var guild));
        Assert.Null(guild);
    }
}
