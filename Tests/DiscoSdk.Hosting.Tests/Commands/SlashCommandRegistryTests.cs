using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Hosting.Tests.Commands.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandRegistryTests
{
    [Fact]
    public async Task Scanner_WithMultipleHandlerClasses_RegistersAllCommands()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(TestHandlerA), typeof(TestHandlerB) }).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());

        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();
        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var commandNames = factory.GlobalCommands.Select(c => c.Name).ToList();
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
