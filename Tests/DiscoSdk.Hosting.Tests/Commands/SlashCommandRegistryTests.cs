using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandRegistryTests
{
    [Fact]
    public void Constructor_WithMultipleHandlerClasses_RegistersAllCommands()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, [typeof(TestHandlerA), typeof(TestHandlerB)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        // Act
        registry.OnCommandsUpdateWindowOpened(client, container);

        // Assert — both handler classes' commands must be in the container
        var commandNames = container.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("test-handler-a-cmd", commandNames);
        Assert.Contains("test-handler-b-cmd", commandNames);
    }
}

public class TestHandlerA : SlashCommandHandler
{
    [SlashCommand("test-handler-a-cmd", "Command from handler A")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}

public class TestHandlerB : SlashCommandHandler
{
    [SlashCommand("test-handler-b-cmd", "Command from handler B")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}
