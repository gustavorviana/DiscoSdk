using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Hosting.Tests.Commands.TestHelpers;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class OnDemandRegistrationTests
{
    private static (CommandAutoRegisterModule module, CommandRegistry catalog) BuildSlash(params Type[] handlerTypes)
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)handlerTypes).ApplyTo(builder, services);
        var registry = builder.Build();
        return (new CommandAutoRegisterModule(registry), registry);
    }

    private static (CommandAutoRegisterModule module, CommandRegistry catalog) BuildContext(params Type[] handlerTypes)
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new ContextMenuCommandScanner((IEnumerable<Type>)handlerTypes).ApplyTo(builder, services);
        var registry = builder.Build();
        return (new CommandAutoRegisterModule(registry), registry);
    }

    // ── Slash commands ──

    [Fact]
    public async Task SlashScanner_OnDemandWithoutGuildIds_DoesNotAutoRegisterGloballyAsync()
    {
        var (module, _) = BuildSlash(typeof(OnDemandOnlySlashHandler));
        var factory = new CapturingCommandUpdateFactory();

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        Assert.DoesNotContain("premium-slash", factory.GlobalCommands.Select(c => c.Name));
        Assert.Empty(factory.GuildCommands);
    }

    [Fact]
    public async Task SlashScanner_OnDemandWithoutGuildIds_StillEntersCatalogAsync()
    {
        var (module, catalog) = BuildSlash(typeof(OnDemandOnlySlashHandler));

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), new CapturingCommandUpdateFactory());

        var cmd = catalog.Get("premium-slash", ApplicationCommandType.ChatInput);
        Assert.Equal("premium-slash", cmd.Name);
        Assert.Contains(cmd, catalog.GetOnDemand(ApplicationCommandType.ChatInput));
    }

    [Fact]
    public async Task SlashScanner_OnDemandWithGuildIds_AutoRegistersInGuildsAndEntersCatalogAsync()
    {
        var (module, catalog) = BuildSlash(typeof(OnDemandGuildSlashHandler));
        var factory = new CapturingCommandUpdateFactory();

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        Assert.Contains(
            factory.GuildCommands.Values.SelectMany(v => v),
            c => c.Name == "beta-slash");
        Assert.Contains(catalog.GetOnDemand(ApplicationCommandType.ChatInput), c => c.Name == "beta-slash");
        Assert.DoesNotContain("beta-slash", factory.GlobalCommands.Select(c => c.Name));
    }

    [Fact]
    public async Task SlashScanner_NonOnDemandCommand_EntersCatalogButNotOnDemandViewAsync()
    {
        var (module, catalog) = BuildSlash(typeof(PlainSlashHandler));

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), new CapturingCommandUpdateFactory());

        Assert.Contains(catalog.GetAll(ApplicationCommandType.ChatInput), c => c.Name == "plain-slash");
        Assert.DoesNotContain(catalog.GetOnDemand(ApplicationCommandType.ChatInput), c => c.Name == "plain-slash");
    }

    // ── Context menu commands ──

    [Fact]
    public async Task ContextScanner_OnDemandUserCommand_DoesNotAutoRegisterGloballyAsync()
    {
        var (module, catalog) = BuildContext(typeof(OnDemandOnlyUserHandler));
        var factory = new CapturingCommandUpdateFactory();

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        Assert.DoesNotContain("premium_user_ctx", factory.GlobalCommands.Select(c => c.Name));
        Assert.Contains(catalog.GetOnDemand(ApplicationCommandType.User), c => c.Name == "premium_user_ctx");
    }

    [Fact]
    public async Task ContextScanner_OnDemandMessageCommand_DoesNotAutoRegisterGloballyAsync()
    {
        var (module, catalog) = BuildContext(typeof(OnDemandOnlyMessageHandler));
        var factory = new CapturingCommandUpdateFactory();

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        Assert.DoesNotContain("premium_msg_ctx", factory.GlobalCommands.Select(c => c.Name));
        Assert.Contains(catalog.GetOnDemand(ApplicationCommandType.Message), c => c.Name == "premium_msg_ctx");
    }

    [Fact]
    public async Task ContextScanner_OnDemandWithGuildIds_AutoRegistersInGuildsAndEntersCatalogAsync()
    {
        var (module, catalog) = BuildContext(typeof(OnDemandGuildUserHandler));
        var factory = new CapturingCommandUpdateFactory();

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        Assert.Contains(
            factory.GuildCommands.Values.SelectMany(v => v),
            c => c.Name == "beta_user_ctx");
        Assert.Contains(catalog.GetOnDemand(ApplicationCommandType.User), c => c.Name == "beta_user_ctx");
        Assert.DoesNotContain("beta_user_ctx", factory.GlobalCommands.Select(c => c.Name));
    }

    // ── Mixed scenarios ──

    [Fact]
    public async Task SlashScanner_MixedOnDemandAndPlain_AutoRegistersOnlyPlainAsync()
    {
        var (module, catalog) = BuildSlash(typeof(PlainSlashHandler), typeof(OnDemandOnlySlashHandler));
        var factory = new CapturingCommandUpdateFactory();

        await module.OnCommandsUpdateWindowOpenedAsync(Substitute.For<IDiscordClient>(), factory);

        var globalNames = factory.GlobalCommands.Select(c => c.Name).ToList();
        Assert.Contains("plain-slash", globalNames);
        Assert.DoesNotContain("premium-slash", globalNames);
        Assert.Equal(2, catalog.GetAll(ApplicationCommandType.ChatInput).Count);
    }

    // ── Test handler classes ──

    private class OnDemandOnlySlashHandler : SlashCommandHandler
    {
        [SlashCommand("premium-slash", "Premium-only slash command")]
        [OnDemand]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    private class OnDemandGuildSlashHandler : SlashCommandHandler
    {
        [SlashCommand("beta-slash", "Beta slash command", GuildIds = new[] { "111111111111111111" })]
        [OnDemand]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    private class PlainSlashHandler : SlashCommandHandler
    {
        [SlashCommand("plain-slash", "Plain global slash command")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    private class OnDemandOnlyUserHandler : UserContextMenuHandler
    {
        [ContextMenuCommand("premium_user_ctx")]
        [OnDemand]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    private class OnDemandOnlyMessageHandler : MessageContextMenuHandler
    {
        [ContextMenuCommand("premium_msg_ctx")]
        [OnDemand]
        protected Task ExecuteAsync(IMessageCommandContext context) => Task.CompletedTask;
    }

    private class OnDemandGuildUserHandler : UserContextMenuHandler
    {
        [ContextMenuCommand("beta_user_ctx", GuildIds = new[] { "111111111111111111" })]
        [OnDemand]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }
}
