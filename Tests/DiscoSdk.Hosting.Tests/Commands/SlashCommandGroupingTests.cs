using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Hosting.Tests.Commands.TestHelpers;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandGroupingTests
{
    [Fact]
    public async Task Constructor_WithSubCommands_GroupsByCommandName()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable) }).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var commandNames = factory.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("config", commandNames);
    }

    [Fact]
    public async Task Constructor_WithSubCommands_GeneratesSubCommandOptions()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable) }).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var configCommand = factory.GlobalCommands.Single(c => c.Name == "config");
        Assert.NotNull(configCommand.Options);

        var subCommandNames = configCommand.Options!
            .Where(o => o.Type == SlashCommandOptionType.SubCommand)
            .Select(o => o.Name)
            .ToList();

        Assert.Contains("set", subCommandNames);
        Assert.Contains("get", subCommandNames);
    }

    [Fact]
    public async Task Constructor_WithSubCommandGroup_GeneratesGroupOptions()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable) }).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var configCommand = factory.GlobalCommands.Single(c => c.Name == "config");
        Assert.NotNull(configCommand.Options);

        var groupOption = configCommand.Options!
            .SingleOrDefault(o => o.Type == SlashCommandOptionType.SubCommandGroup && o.Name == "notifications");

        Assert.NotNull(groupOption);
        Assert.NotNull(groupOption!.Options);

        var subInGroup = groupOption.Options!.Select(o => o.Name).ToList();
        Assert.Contains("enable", subInGroup);
    }

    [Fact]
    public async Task Constructor_SubCommandWithOptions_OptionsAreLeafOfSubCommand()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable) }).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var configCommand = factory.GlobalCommands.Single(c => c.Name == "config");
        var setSubCommand = configCommand.Options!
            .Single(o => o.Type == SlashCommandOptionType.SubCommand && o.Name == "set");

        Assert.NotNull(setSubCommand.Options);
        var keyOption = setSubCommand.Options!.Single(o => o.Name == "key");
        Assert.Equal(SlashCommandOptionType.String, keyOption.Type);
        Assert.True(keyOption.Required);
    }

    [Fact]
    public async Task Constructor_CrossHandlerGrouping_MethodsFromDifferentHandlersGroupedTogether()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable) }).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var configCommands = factory.GlobalCommands.Where(c => c.Name == "config").ToList();
        Assert.Single(configCommands);

        var configCommand = configCommands[0];
        Assert.NotNull(configCommand.Options);

        Assert.Equal(3, configCommand.Options!.Length);
    }

    [Fact]
    public async Task Constructor_FlatCommandsStillWork_AlongsideGroupedCommands()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(TestHandlerA), typeof(TestHandlerB), typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable) }).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        var commandNames = factory.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("test-handler-a-cmd", commandNames);
        Assert.Contains("test-handler-b-cmd", commandNames);
    }
}

// --- Test handler classes for grouping ---
// These all contribute subcommands to the "config" parent command,
// demonstrating cross-handler grouping.

public class GroupTestHandlerSet : SlashCommandHandler
{
    [SlashCommand("config", "Configuration commands")]
    [SubCommand("set", "Set a configuration value")]
    [SlashOption(SlashCommandOptionType.String, name: "key", description: "The key to set", required: true)]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}

public class GroupTestHandlerGet : SlashCommandHandler
{
    [SlashCommand("config", "Configuration commands")]
    [SubCommand("get", "Get a configuration value")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}

public class GroupTestHandlerNotifEnable : SlashCommandHandler
{
    [SlashCommand("config", "Configuration commands")]
    [SubCommandGroup("notifications", "Notification settings")]
    [SubCommand("enable", "Enable notifications")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}
