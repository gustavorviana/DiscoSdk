using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Hosting.Tests.Commands.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class ContextMenuCommandRegistryTests
{
    private static (ContextMenuCommandDispatcher dispatcher, CommandAutoRegisterModule module, IServiceProvider services)
        BuildHarness(params Type[] handlerTypes)
        => BuildHarness<IUserCommandContext>(handlerTypes);

    private static (ContextMenuCommandDispatcher dispatcher, CommandAutoRegisterModule module, IServiceProvider services)
        BuildHarness<TContext>(Type[] handlerTypes) where TContext : class, IContext
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new ContextMenuCommandScanner((IEnumerable<Type>)handlerTypes).ApplyTo(builder, services);
        var registry = builder.Build();

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns((IContext)Substitute.For<TContext>());
        services.AddScoped(_ => contextProvider);

        var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;
        return (new ContextMenuCommandDispatcher(registry), new CommandAutoRegisterModule(registry), sp);
    }

    private static void Scan(params Type[] handlerTypes)
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new ContextMenuCommandScanner((IEnumerable<Type>)handlerTypes).ApplyTo(builder, services);
        // builder.Build() not strictly required here — scanner errors fire at ApplyTo time.
    }

    // --- Scanning Tests ---

    [Fact]
    public async Task Scanner_DiscoversUserCommandHandlers_RegistersUserCommands()
    {
        var (_, module, _) = BuildHarness(typeof(TestUserHandler));
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var names = factory.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("report_user", names);
    }

    [Fact]
    public async Task Scanner_DiscoversMessageCommandHandlers_RegistersMessageCommands()
    {
        var (_, module, _) = BuildHarness(typeof(TestMessageHandler));
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var names = factory.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("bookmark_message", names);
    }

    [Fact]
    public async Task Scanner_ScansMultipleHandlerTypes_RegistersAll()
    {
        var (_, module, _) = BuildHarness(typeof(TestUserHandler), typeof(TestMessageHandler), typeof(AnotherUserHandler));
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var names = factory.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("report_user", names);
        Assert.Contains("bookmark_message", names);
        Assert.Contains("greet_user", names);
    }

    [Fact]
    public async Task AutoRegister_UserCommand_SetsCorrectType()
    {
        var (_, module, _) = BuildHarness(typeof(TestUserHandler));
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var cmd = factory.GlobalCommands.Single(c => c.Name == "report_user");
        Assert.Equal(DiscoSdk.Models.Enums.ApplicationCommandType.User, cmd.Type);
    }

    [Fact]
    public async Task AutoRegister_MessageCommand_SetsCorrectType()
    {
        var (_, module, _) = BuildHarness(typeof(TestMessageHandler));
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var cmd = factory.GlobalCommands.Single(c => c.Name == "bookmark_message");
        Assert.Equal(DiscoSdk.Models.Enums.ApplicationCommandType.Message, cmd.Type);
    }

    // --- Validation Tests ---

    [Fact]
    public void Scanner_DuplicateUserCommandName_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Scan(typeof(TestUserHandler), typeof(DuplicateUserHandler)));
    }

    [Fact]
    public void Scanner_DuplicateMessageCommandName_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Scan(typeof(TestMessageHandler), typeof(DuplicateMessageHandler)));
    }

    [Fact]
    public void Scanner_SameNameUserAndMessageCommands_AllowedByDifferentTypes()
    {
        // User and Message context menus can coexist with the same name —
        // they live in different buckets keyed by (name, type).
        Scan(typeof(SameNameUserHandler), typeof(SameNameMessageHandler));
    }

    // --- Routing Tests ---

    private static string? _lastInvokedMethod;

    private static void ResetTracker() => _lastInvokedMethod = null;

    private static readonly Type[] RoutingTypes =
        [typeof(RoutingUserHandler), typeof(RoutingMessageHandler), typeof(AnotherRoutingUserHandler)];

    [Fact]
    public async Task HandleAsync_UserCommand_RoutesToCorrectHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness(RoutingTypes);

        var context = Substitute.For<IUserCommandContext>();
        context.Name.Returns("route_user_a");
        var handler = (IDiscordEventHandler<IUserCommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RoutingUserHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UserCommand_RoutesToOtherHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness(RoutingTypes);

        var context = Substitute.For<IUserCommandContext>();
        context.Name.Returns("route_user_b");
        var handler = (IDiscordEventHandler<IUserCommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("AnotherRoutingUserHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_MessageCommand_RoutesToCorrectHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness<IMessageCommandContext>(RoutingTypes);

        var context = Substitute.For<IMessageCommandContext>();
        context.Name.Returns("route_message_a");
        var handler = (IDiscordEventHandler<IMessageCommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RoutingMessageHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownUserCommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness(RoutingTypes);

        var context = Substitute.For<IUserCommandContext>();
        context.Name.Returns("Nonexistent");
        var handler = (IDiscordEventHandler<IUserCommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownMessageCommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness(RoutingTypes);

        var context = Substitute.For<IMessageCommandContext>();
        context.Name.Returns("Nonexistent");
        var handler = (IDiscordEventHandler<IMessageCommandContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    // --- Implements Correct Interfaces ---

    [Fact]
    public void Dispatcher_ImplementsIUserCommandHandler()
    {
        var registry = new CommandRegistryBuilder().Build();
        var dispatcher = new ContextMenuCommandDispatcher(registry);
        Assert.IsAssignableFrom<IUserCommandHandler>(dispatcher);
    }

    [Fact]
    public void Dispatcher_ImplementsIMessageCommandHandler()
    {
        var registry = new CommandRegistryBuilder().Build();
        var dispatcher = new ContextMenuCommandDispatcher(registry);
        Assert.IsAssignableFrom<IMessageCommandHandler>(dispatcher);
    }

    [Fact]
    public void AutoRegisterModule_ImplementsICommandsUpdateWindowModule()
    {
        var registry = new CommandRegistryBuilder().Build();
        var module = new CommandAutoRegisterModule(registry);
        Assert.IsAssignableFrom<DiscoSdk.Modules.ICommandsUpdateWindowModule>(module);
    }

    // --- Test Handler Classes ---

    public class TestUserHandler : UserContextMenuHandler
    {
        [UserCommand("report_user")]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    public class AnotherUserHandler : UserContextMenuHandler
    {
        [UserCommand("greet_user")]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    public class TestMessageHandler : MessageContextMenuHandler
    {
        [MessageCommand("bookmark_message")]
        protected Task ExecuteAsync(IMessageCommandContext context) => Task.CompletedTask;
    }

    public class DuplicateUserHandler : UserContextMenuHandler
    {
        [UserCommand("report_user")]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    public class DuplicateMessageHandler : MessageContextMenuHandler
    {
        [MessageCommand("bookmark_message")]
        protected Task ExecuteAsync(IMessageCommandContext context) => Task.CompletedTask;
    }

    public class SameNameUserHandler : UserContextMenuHandler
    {
        [UserCommand("same_name")]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    public class SameNameMessageHandler : MessageContextMenuHandler
    {
        [MessageCommand("same_name")]
        protected Task ExecuteAsync(IMessageCommandContext context) => Task.CompletedTask;
    }

    public class RoutingUserHandler : UserContextMenuHandler
    {
        [UserCommand("route_user_a")]
        protected Task ExecuteAsync(IUserCommandContext context)
        {
            _lastInvokedMethod = "RoutingUserHandler.Execute";
            return Task.CompletedTask;
        }
    }

    public class AnotherRoutingUserHandler : UserContextMenuHandler
    {
        [UserCommand("route_user_b")]
        protected Task ExecuteAsync(IUserCommandContext context)
        {
            _lastInvokedMethod = "AnotherRoutingUserHandler.Execute";
            return Task.CompletedTask;
        }
    }

    public class RoutingMessageHandler : MessageContextMenuHandler
    {
        [MessageCommand("route_message_a")]
        protected Task ExecuteAsync(IMessageCommandContext context)
        {
            _lastInvokedMethod = "RoutingMessageHandler.Execute";
            return Task.CompletedTask;
        }
    }
}
