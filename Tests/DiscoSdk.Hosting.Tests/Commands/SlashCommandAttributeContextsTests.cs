using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Hosting.Tests.Commands.TestHelpers;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandAttributeContextsTests
{
    [Fact]
    public async Task IntegrationTypes_FromAttribute_FlowsToRegisteredCommandAsync()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)[typeof(IntegrationTypesHandler)]).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());

        var factory = new CapturingCommandUpdateFactory();
        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        var cmd = Assert.Single(factory.GlobalCommands, c => c.Name == "user-and-guild");
        Assert.NotNull(cmd.IntegrationTypes);
        Assert.Equal(
            [ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall],
            cmd.IntegrationTypes);
    }

    [Fact]
    public async Task Contexts_FromAttribute_FlowsToRegisteredCommandAsync()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)[typeof(ContextsHandler)]).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());

        var factory = new CapturingCommandUpdateFactory();
        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        var cmd = Assert.Single(factory.GlobalCommands, c => c.Name == "dm-only");
        Assert.NotNull(cmd.Contexts);
        Assert.Equal(
            [InteractionContextType.BotDm, InteractionContextType.PrivateChannel],
            cmd.Contexts);
    }

    [Fact]
    public async Task IntegrationTypesAndContexts_OmittedFromAttribute_StayNullAsync()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)[typeof(PlainHandler)]).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());

        var factory = new CapturingCommandUpdateFactory();
        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        var cmd = Assert.Single(factory.GlobalCommands, c => c.Name == "plain");
        Assert.Null(cmd.IntegrationTypes);
        Assert.Null(cmd.Contexts);
    }

    [Fact]
    public async Task IntegrationTypesAndContexts_OnGroupParent_FlowToRegisteredCommandAsync()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)[typeof(GroupedHandler)]).ApplyTo(builder, services);
        var module = new CommandAutoRegisterModule(builder.Build());

        var factory = new CapturingCommandUpdateFactory();
        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        var cmd = Assert.Single(factory.GlobalCommands, c => c.Name == "music");
        Assert.Equal([ApplicationIntegrationType.GuildInstall], cmd.IntegrationTypes);
        Assert.Equal([InteractionContextType.Guild], cmd.Contexts);
    }

    public class IntegrationTypesHandler : SlashCommandHandler
    {
        [SlashCommand("user-and-guild", "Installable anywhere.",
            IntegrationTypes = [ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall])]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    public class ContextsHandler : SlashCommandHandler
    {
        [SlashCommand("dm-only", "Usable in DMs only.",
            Contexts = [InteractionContextType.BotDm, InteractionContextType.PrivateChannel])]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    public class PlainHandler : SlashCommandHandler
    {
        [SlashCommand("plain", "No contexts or integration types.")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    public class GroupedHandler : SlashCommandHandler
    {
        [SlashCommand("music", "Music controls.",
            IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
            Contexts = [InteractionContextType.Guild])]
        [SubCommand("play", "Play a song.")]
        protected Task PlayAsync(ICommandContext context) => Task.CompletedTask;

        [SlashCommand("music", "Music controls.",
            IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
            Contexts = [InteractionContextType.Guild])]
        [SubCommand("stop", "Stop playback.")]
        protected Task StopAsync(ICommandContext context) => Task.CompletedTask;
    }
}
