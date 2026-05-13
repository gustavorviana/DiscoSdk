using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandGroupingTests
{
    [Fact]
    public void Constructor_WithSubCommands_GroupsByCommandName()
    {
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, [typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var commandNames = container.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("config", commandNames);
    }

    [Fact]
    public void Constructor_WithSubCommands_GeneratesSubCommandOptions()
    {
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, [typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var configCommand = container.GlobalCommands.Single(c => c.Name == "config");
        Assert.NotNull(configCommand.Options);

        var subCommandNames = configCommand.Options!
            .Where(o => o.Type == SlashCommandOptionType.SubCommand)
            .Select(o => o.Name)
            .ToList();

        Assert.Contains("set", subCommandNames);
        Assert.Contains("get", subCommandNames);
    }

    [Fact]
    public void Constructor_WithSubCommandGroup_GeneratesGroupOptions()
    {
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, [typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var configCommand = container.GlobalCommands.Single(c => c.Name == "config");
        Assert.NotNull(configCommand.Options);

        var groupOption = configCommand.Options!
            .SingleOrDefault(o => o.Type == SlashCommandOptionType.SubCommandGroup && o.Name == "notifications");

        Assert.NotNull(groupOption);
        Assert.NotNull(groupOption!.Options);

        var subInGroup = groupOption.Options!.Select(o => o.Name).ToList();
        Assert.Contains("enable", subInGroup);
    }

    [Fact]
    public void Constructor_SubCommandWithOptions_OptionsAreLeafOfSubCommand()
    {
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, [typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var configCommand = container.GlobalCommands.Single(c => c.Name == "config");
        var setSubCommand = configCommand.Options!
            .Single(o => o.Type == SlashCommandOptionType.SubCommand && o.Name == "set");

        // The "key" option should be a leaf option of the "set" subcommand
        Assert.NotNull(setSubCommand.Options);
        var keyOption = setSubCommand.Options!.Single(o => o.Name == "key");
        Assert.Equal(SlashCommandOptionType.String, keyOption.Type);
        Assert.True(keyOption.Required);
    }

    [Fact]
    public void Constructor_CrossHandlerGrouping_MethodsFromDifferentHandlersGroupedTogether()
    {
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, [typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        // "config" should appear only once, containing subcommands from different handler classes
        var configCommands = container.GlobalCommands.Where(c => c.Name == "config").ToList();
        Assert.Single(configCommands);

        var configCommand = configCommands[0];
        Assert.NotNull(configCommand.Options);

        // Should have: "set" (SubCommand), "get" (SubCommand), "notifications" (SubCommandGroup)
        Assert.Equal(3, configCommand.Options!.Length);
    }

    [Fact]
    public void Constructor_FlatCommandsStillWork_AlongsideGroupedCommands()
    {
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, [typeof(TestHandlerA), typeof(TestHandlerB), typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable)]);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        var commandNames = container.GlobalCommands.Select(c => c.Name).ToList();
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
