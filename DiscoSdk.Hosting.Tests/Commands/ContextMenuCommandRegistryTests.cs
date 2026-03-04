using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class ContextMenuCommandRegistryTests
{
    // --- Scanning Tests ---

    [Fact]
    public void Constructor_ScansUserCommandHandlers_RegistersUserCommands()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services,
            [typeof(TestUserHandler)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var names = container.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("report_user", names);
    }

    [Fact]
    public void Constructor_ScansMessageCommandHandlers_RegistersMessageCommands()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services,
            [typeof(TestMessageHandler)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var names = container.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("bookmark_message", names);
    }

    [Fact]
    public void Constructor_ScansMultipleHandlerTypes_RegistersAll()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services,
            [typeof(TestUserHandler), typeof(TestMessageHandler), typeof(AnotherUserHandler)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var names = container.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("report_user", names);
        Assert.Contains("bookmark_message", names);
        Assert.Contains("greet_user", names);
    }

    [Fact]
    public void OnCommandsUpdateWindowOpened_UserCommand_SetsCorrectType()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services,
            [typeof(TestUserHandler)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var cmd = container.GlobalCommands.Single(c => c.Name == "report_user");
        Assert.Equal(DiscoSdk.Models.Enums.ApplicationCommandType.User, cmd.Type);
    }

    [Fact]
    public void OnCommandsUpdateWindowOpened_MessageCommand_SetsCorrectType()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services,
            [typeof(TestMessageHandler)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var cmd = container.GlobalCommands.Single(c => c.Name == "bookmark_message");
        Assert.Equal(DiscoSdk.Models.Enums.ApplicationCommandType.Message, cmd.Type);
    }

    // --- Validation Tests ---

    [Fact]
    public void Constructor_DuplicateUserCommandName_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
            new ContextMenuCommandRegistry(services,
                [typeof(TestUserHandler), typeof(DuplicateUserHandler)]));
    }

    [Fact]
    public void Constructor_DuplicateMessageCommandName_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
            new ContextMenuCommandRegistry(services,
                [typeof(TestMessageHandler), typeof(DuplicateMessageHandler)]));
    }

    [Fact]
    public void Constructor_SameNameUserAndMessageCommands_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services,
            [typeof(SameNameUserHandler), typeof(SameNameMessageHandler)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        Assert.Throws<InvalidOperationException>(() =>
            registry.OnCommandsUpdateWindowOpened(client, container));
    }

    // --- Routing Tests ---

    private static string? _lastInvokedMethod;

    private static void ResetTracker() => _lastInvokedMethod = null;

    private static readonly Type[] RoutingTypes =
        [typeof(RoutingUserHandler), typeof(RoutingMessageHandler), typeof(AnotherRoutingUserHandler)];

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        _ = new ContextMenuCommandRegistry(services, RoutingTypes);

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns(Substitute.For<IUserCommandContext>());
        services.AddScoped(_ => contextProvider);

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }

    [Fact]
    public async Task HandleAsync_UserCommand_RoutesToCorrectHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, RoutingTypes);

        var sp = BuildServiceProvider();
        var context = Substitute.For<IUserCommandContext>();
        context.Name.Returns("route_user_a");
        var handler = (IDiscordEventHandler<IUserCommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RoutingUserHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UserCommand_RoutesToOtherHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, RoutingTypes);

        var sp = BuildServiceProvider();
        var context = Substitute.For<IUserCommandContext>();
        context.Name.Returns("route_user_b");
        var handler = (IDiscordEventHandler<IUserCommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("AnotherRoutingUserHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_MessageCommand_RoutesToCorrectHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, RoutingTypes);

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns(Substitute.For<IMessageCommandContext>());
        var msgServices = new ServiceCollection();
        _ = new ContextMenuCommandRegistry(msgServices, RoutingTypes);
        msgServices.AddScoped(_ => contextProvider);
        var sp = msgServices.BuildServiceProvider().CreateScope().ServiceProvider;

        var context = Substitute.For<IMessageCommandContext>();
        context.Name.Returns("route_message_a");
        var handler = (IDiscordEventHandler<IMessageCommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("RoutingMessageHandler.Execute", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownUserCommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, RoutingTypes);

        var sp = BuildServiceProvider();
        var context = Substitute.For<IUserCommandContext>();
        context.Name.Returns("Nonexistent");
        var handler = (IDiscordEventHandler<IUserCommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAsync_UnknownMessageCommand_DoesNotThrowAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, RoutingTypes);

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns(Substitute.For<IMessageCommandContext>());
        var msgServices = new ServiceCollection();
        _ = new ContextMenuCommandRegistry(msgServices, RoutingTypes);
        msgServices.AddScoped(_ => contextProvider);
        var sp = msgServices.BuildServiceProvider().CreateScope().ServiceProvider;

        var context = Substitute.For<IMessageCommandContext>();
        context.Name.Returns("Nonexistent");
        var handler = (IDiscordEventHandler<IMessageCommandContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    // --- Implements Correct Interfaces ---

    [Fact]
    public void Registry_ImplementsILifetimeDiscoModule()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, Array.Empty<Assembly>());

        Assert.IsAssignableFrom<DiscoSdk.Modules.ICommandsUpdateWindowModule>(registry);
    }

    [Fact]
    public void Registry_ImplementsIUserCommandHandler()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, Array.Empty<Assembly>());

        Assert.IsAssignableFrom<IUserCommandHandler>(registry);
    }

    [Fact]
    public void Registry_ImplementsIMessageCommandHandler()
    {
        var services = new ServiceCollection();
        var registry = new ContextMenuCommandRegistry(services, Array.Empty<Assembly>());

        Assert.IsAssignableFrom<IMessageCommandHandler>(registry);
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
