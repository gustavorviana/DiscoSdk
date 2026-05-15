using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Commands;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandRoutingTests
{
    private static readonly Type[] RoutingHandlerTypes =
        [typeof(RouteFlatHandler), typeof(RouteGroupSubA), typeof(RouteGroupSubB), typeof(RouteGroupNested)];

    private static string? _lastInvokedMethod;

    private static void ResetTracker() => _lastInvokedMethod = null;

    private static ICommandContext CreateMockContext(string name, string? subcommand = null, string? subcommandGroup = null)
    {
        var context = Substitute.For<ICommandContext>();
        context.Name.Returns(name);
        context.Subcommand.Returns(subcommand);
        context.SubcommandGroup.Returns(subcommandGroup);
        context.Options.Returns(Array.Empty<IRootCommandOption>());
        return context;
    }

    private static (SlashCommandDispatcher dispatcher, IServiceProvider services) BuildDispatcher()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)RoutingHandlerTypes).ApplyTo(builder, services);
        var registry = builder.Build();

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns(Substitute.For<IInteractionContext>());
        services.AddScoped(_ => contextProvider);

        var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;
        return (new SlashCommandDispatcher(registry), sp);
    }

    [Fact]
    public async Task HandleAsync_FlatCommand_RoutesToFlatHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, sp) = BuildDispatcher();
        var context = CreateMockContext("route-flat");
        var handler = (IDiscordEventHandler<ICommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteFlatHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_OneLevelSubcommand_RoutesToSubcommandHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, sp) = BuildDispatcher();
        var context = CreateMockContext("route-grouped", subcommand: "sub-a");
        var handler = (IDiscordEventHandler<ICommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteGroupSubA.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_OneLevelSubcommand_OtherSubcommand_RoutesCorrectlyAsync()
    {
        ResetTracker();
        var (dispatcher, sp) = BuildDispatcher();
        var context = CreateMockContext("route-grouped", subcommand: "sub-b");
        var handler = (IDiscordEventHandler<ICommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteGroupSubB.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_TwoLevelSubcommand_RoutesToGroupedSubcommandHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, sp) = BuildDispatcher();
        var context = CreateMockContext("route-grouped", subcommand: "nested-sub", subcommandGroup: "nested-group");
        var handler = (IDiscordEventHandler<ICommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteGroupNested.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownCommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var (dispatcher, sp) = BuildDispatcher();
        var context = CreateMockContext("nonexistent");
        var handler = (IDiscordEventHandler<ICommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownSubcommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var (dispatcher, sp) = BuildDispatcher();
        var context = CreateMockContext("route-grouped", subcommand: "nonexistent");
        var handler = (IDiscordEventHandler<ICommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    // --- Test handlers for routing ---

    public class RouteFlatHandler : SlashCommandHandler
    {
        [SlashCommand("route-flat", "A flat command for routing test")]
        protected Task ExecuteAsync(ICommandContext context)
        {
            _lastInvokedMethod = "RouteFlatHandler.Execute";
            return Task.CompletedTask;
        }
    }

    public class RouteGroupSubA : SlashCommandHandler
    {
        [SlashCommand("route-grouped", "Grouped command")]
        [SubCommand("sub-a", "Subcommand A")]
        protected Task ExecuteAsync(ICommandContext context)
        {
            _lastInvokedMethod = "RouteGroupSubA.Execute";
            return Task.CompletedTask;
        }
    }

    public class RouteGroupSubB : SlashCommandHandler
    {
        [SlashCommand("route-grouped", "Grouped command")]
        [SubCommand("sub-b", "Subcommand B")]
        protected Task ExecuteAsync(ICommandContext context)
        {
            _lastInvokedMethod = "RouteGroupSubB.Execute";
            return Task.CompletedTask;
        }
    }

    public class RouteGroupNested : SlashCommandHandler
    {
        [SlashCommand("route-grouped", "Grouped command")]
        [SubCommandGroup("nested-group", "A nested group")]
        [SubCommand("nested-sub", "A nested subcommand")]
        protected Task ExecuteAsync(ICommandContext context)
        {
            _lastInvokedMethod = "RouteGroupNested.Execute";
            return Task.CompletedTask;
        }
    }
}
