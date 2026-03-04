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

    private static IServiceProvider BuildServiceProvider(SlashCommandRegistry registry)
    {
        var services = new ServiceCollection();

        // Re-create registry to register handler types in DI
        _ = new SlashCommandRegistry(services, RoutingHandlerTypes);

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns(Substitute.For<IInteractionContext>());
        services.AddScoped(_ => contextProvider);

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }

    [Fact]
    public async Task HandleAsync_FlatCommand_RoutesToFlatHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, RoutingHandlerTypes);

        var sp = BuildServiceProvider(registry);
        var context = CreateMockContext("route-flat");
        var handler = (IDiscordEventHandler<ICommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteFlatHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_OneLevelSubcommand_RoutesToSubcommandHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, RoutingHandlerTypes);

        var sp = BuildServiceProvider(registry);
        var context = CreateMockContext("route-grouped", subcommand: "sub-a");
        var handler = (IDiscordEventHandler<ICommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteGroupSubA.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_OneLevelSubcommand_OtherSubcommand_RoutesCorrectlyAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, RoutingHandlerTypes);

        var sp = BuildServiceProvider(registry);
        var context = CreateMockContext("route-grouped", subcommand: "sub-b");
        var handler = (IDiscordEventHandler<ICommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteGroupSubB.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_TwoLevelSubcommand_RoutesToGroupedSubcommandHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, RoutingHandlerTypes);

        var sp = BuildServiceProvider(registry);
        var context = CreateMockContext("route-grouped", subcommand: "nested-sub", subcommandGroup: "nested-group");
        var handler = (IDiscordEventHandler<ICommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RouteGroupNested.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownCommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, RoutingHandlerTypes);

        var sp = BuildServiceProvider(registry);
        var context = CreateMockContext("nonexistent");
        var handler = (IDiscordEventHandler<ICommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownSubcommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, RoutingHandlerTypes);

        var sp = BuildServiceProvider(registry);
        var context = CreateMockContext("route-grouped", subcommand: "nonexistent");
        var handler = (IDiscordEventHandler<ICommandContext>)registry;

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
