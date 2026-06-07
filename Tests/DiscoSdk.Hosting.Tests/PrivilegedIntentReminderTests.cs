using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests;

/// <summary>
/// Verifies <see cref="DiscordClient.LogPrivilegedIntentReminder"/> emits a single Information-level
/// reminder when any privileged intent (<c>GuildMembers</c> / <c>GuildPresences</c> /
/// <c>MessageContent</c>) is in the configured intent set, and stays silent otherwise. The log line
/// is the single grep target operators use when chasing a Discord close code 4014.
/// </summary>
public class PrivilegedIntentReminderTests
{
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

    [Fact]
    public void NoPrivilegedIntent_DoesNotLog()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        var client = NewClient(DiscordIntent.Guilds | DiscordIntent.GuildMessages, logger);

        client.LogPrivilegedIntentReminder();

        logger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData(DiscordIntent.GuildMembers)]
    [InlineData(DiscordIntent.GuildPresences)]
    [InlineData(DiscordIntent.MessageContent)]
    [InlineData(DiscordIntent.GuildMembers | DiscordIntent.MessageContent)]
    [InlineData(DiscordIntent.GuildMembers | DiscordIntent.GuildPresences | DiscordIntent.MessageContent)]
    public void PrivilegedIntent_LogsOnceAtInformation(DiscordIntent privileged)
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        var client = NewClient(DiscordIntent.Guilds | privileged, logger);

        client.LogPrivilegedIntentReminder();

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
