using DiscoSdk;
using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Contexts.Messages;
using DiscoSdk.Events;
using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Events;

/// <summary>
/// Verifies registration-time intent guard for event handlers. When a host wires up a handler whose
/// marker interface declares <see cref="RequiresIntentAttribute"/> and the client's intent bitmask
/// does not satisfy any of the required groups, the dispatcher logs a single Warning per handler
/// type per process. Discord drops the matching events silently in that case; the diagnostic
/// surfaces the gap before the bot sits dormant.
/// </summary>
public class EventHandlerIntentGuardTests : IDisposable
{
    public EventHandlerIntentGuardTests() => EventHandlerIntentWarnTracker.ResetForTests();
    public void Dispose() => EventHandlerIntentWarnTracker.ResetForTests();

    private sealed class MessageCreateNoop : IMessageCreateHandler
    {
        public Task HandleAsync(IMessageCreateContext context, IServiceProvider services) => Task.CompletedTask;
    }

    private sealed class GuildMemberAddNoop : IGuildMemberAddHandler
    {
        public Task HandleAsync(IGuildMemberAddContext context, IServiceProvider services) => Task.CompletedTask;
    }

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
    public void MessageCreateHandler_RegisteredWithoutGuildMessages_LogsWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.Guilds, logger);

        client.InternalInit([], [new MessageCreateNoop()]);

        Assert.True(WarnCalls(logger) > 0);
    }

    [Fact]
    public void MessageCreateHandler_RegisteredWithDirectMessages_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        // DirectMessages alone satisfies the AnyOf group (GuildMessages | DirectMessages).
        var client = NewClient(DiscordIntent.DirectMessages, logger);

        client.InternalInit([], [new MessageCreateNoop()]);

        Assert.Equal(0, WarnCalls(logger));
    }

    [Fact]
    public void MessageCreateHandler_RegisteredWithGuildMessages_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMessages, logger);

        client.InternalInit([], [new MessageCreateNoop()]);

        Assert.Equal(0, WarnCalls(logger));
    }

    [Fact]
    public void GuildMemberAddHandler_RegisteredWithoutGuildMembers_LogsWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.Guilds, logger);

        client.InternalInit([], [new GuildMemberAddNoop()]);

        Assert.True(WarnCalls(logger) > 0);
    }

    [Fact]
    public void GuildMemberAddHandler_RegisteredWithGuildMembers_NoWarning()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.GuildMembers, logger);

        client.InternalInit([], [new GuildMemberAddNoop()]);

        Assert.Equal(0, WarnCalls(logger));
    }

    [Fact]
    public void SameHandlerInterface_TwoInstances_WarnsOnlyOnce()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var client = NewClient(DiscordIntent.Guilds, logger);

        client.InternalInit([], [new MessageCreateNoop(), new MessageCreateNoop()]);

        Assert.Equal(1, WarnCalls(logger));
    }
}
