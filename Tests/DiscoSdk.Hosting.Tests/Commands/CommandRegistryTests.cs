using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Tests.Commands;

public class CommandRegistryTests
{
    private static CommandRegistry BuildRegistryWith(params Type[] handlerTypes)
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();

        // Routing por base class: slash vs context-menu scanners compartilham o mesmo builder,
        // batendo no caminho real de discovery.
        var slashTypes = handlerTypes
            .Where(t => typeof(SlashCommandHandler).IsAssignableFrom(t)).ToArray();
        var ctxTypes = handlerTypes
            .Where(t => typeof(UserContextMenuHandler).IsAssignableFrom(t)
                     || typeof(MessageContextMenuHandler).IsAssignableFrom(t)).ToArray();

        if (slashTypes.Length > 0)
            new SlashCommandScanner((IEnumerable<Type>)slashTypes).ApplyTo(builder, services);
        if (ctxTypes.Length > 0)
            new ContextMenuCommandScanner((IEnumerable<Type>)ctxTypes).ApplyTo(builder, services);

        return builder.Build();
    }

    // ── Lookup (Get / TryGet) ──

    [Fact]
    public void Get_FindsSlashCommandByName()
    {
        var registry = BuildRegistryWith(typeof(PingSlashHandler));

        var cmd = registry.Get("ping", ApplicationCommandType.ChatInput);

        Assert.Equal("ping", cmd.Name);
        Assert.Equal(ApplicationCommandType.ChatInput, cmd.Type);
    }

    [Fact]
    public void Get_FindsUserContextMenuByName()
    {
        var registry = BuildRegistryWith(typeof(ReportUserHandler));

        var cmd = registry.Get("ReportUser", ApplicationCommandType.User);

        Assert.Equal("ReportUser", cmd.Name);
        Assert.Equal(ApplicationCommandType.User, cmd.Type);
    }

    [Fact]
    public void Get_FindsMessageContextMenuByName()
    {
        var registry = BuildRegistryWith(typeof(BookmarkMessageHandler));

        var cmd = registry.Get("BookmarkMessage", ApplicationCommandType.Message);

        Assert.Equal("BookmarkMessage", cmd.Name);
        Assert.Equal(ApplicationCommandType.Message, cmd.Type);
    }

    [Fact]
    public void Get_DistinguishesByType()
    {
        // "shared" exists in all three buckets simultaneously — type disambiguates.
        var registry = BuildRegistryWith(
            typeof(SharedSlashHandler),
            typeof(SharedUserHandler),
            typeof(SharedMessageHandler));

        Assert.Equal(ApplicationCommandType.ChatInput, registry.Get("shared", ApplicationCommandType.ChatInput).Type);
        Assert.Equal(ApplicationCommandType.User, registry.Get("shared", ApplicationCommandType.User).Type);
        Assert.Equal(ApplicationCommandType.Message, registry.Get("shared", ApplicationCommandType.Message).Type);
    }

    [Fact]
    public void Get_ThrowsKeyNotFoundWhenMissing()
    {
        var registry = new CommandRegistryBuilder().Build();
        Assert.Throws<KeyNotFoundException>(() => registry.Get("nope", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public void Get_ThrowsArgumentExceptionForBlankName()
    {
        var registry = new CommandRegistryBuilder().Build();
        Assert.Throws<ArgumentException>(() => registry.Get(" ", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public void TryGet_ReturnsTrueWhenPresent()
    {
        var registry = BuildRegistryWith(typeof(PingSlashHandler));

        Assert.True(registry.TryGet("ping", ApplicationCommandType.ChatInput, out var cmd));
        Assert.Equal("ping", cmd.Name);
    }

    [Fact]
    public void TryGet_ReturnsFalseWhenMissing()
    {
        var registry = new CommandRegistryBuilder().Build();

        Assert.False(registry.TryGet("nope", ApplicationCommandType.ChatInput, out _));
    }

    [Fact]
    public void TryGet_ReturnsFalseForBlankName()
    {
        var registry = new CommandRegistryBuilder().Build();

        Assert.False(registry.TryGet(" ", ApplicationCommandType.ChatInput, out _));
    }

    // ── GetAll(type) ──

    [Fact]
    public void GetAll_ChatInput_ContainsOnlySlashCommands()
    {
        var registry = BuildRegistryWith(typeof(PingSlashHandler), typeof(ReportUserHandler));

        var slash = registry.GetAll(ApplicationCommandType.ChatInput);
        Assert.All(slash, c => Assert.Equal(ApplicationCommandType.ChatInput, c.Type));
        Assert.Contains(slash, c => c.Name == "ping");
    }

    [Fact]
    public void GetAll_User_ContainsOnlyUserContextMenu()
    {
        var registry = BuildRegistryWith(typeof(PingSlashHandler), typeof(ReportUserHandler), typeof(BookmarkMessageHandler));

        var users = registry.GetAll(ApplicationCommandType.User);
        Assert.All(users, c => Assert.Equal(ApplicationCommandType.User, c.Type));
        Assert.Contains(users, c => c.Name == "ReportUser");
        Assert.DoesNotContain(users, c => c.Name == "BookmarkMessage");
    }

    [Fact]
    public void GetAll_Message_ContainsOnlyMessageContextMenu()
    {
        var registry = BuildRegistryWith(typeof(PingSlashHandler), typeof(ReportUserHandler), typeof(BookmarkMessageHandler));

        var messages = registry.GetAll(ApplicationCommandType.Message);
        Assert.All(messages, c => Assert.Equal(ApplicationCommandType.Message, c.Type));
        Assert.Contains(messages, c => c.Name == "BookmarkMessage");
        Assert.DoesNotContain(messages, c => c.Name == "ReportUser");
    }

    [Fact]
    public void GetAll_InvalidType_Throws()
    {
        var registry = new CommandRegistryBuilder().Build();
        Assert.Throws<ArgumentOutOfRangeException>(() => registry.GetAll((ApplicationCommandType)999));
    }

    // ── OnDemand ──

    [Fact]
    public void GetOnDemand_Slash_OnlyReturnsFlaggedSlashCommands()
    {
        var registry = BuildRegistryWith(typeof(PingSlashHandler), typeof(PremiumOnDemandSlashHandler));

        var onDemand = registry.GetOnDemand(ApplicationCommandType.ChatInput);

        Assert.Single(onDemand);
        Assert.Equal("premium", onDemand.Single().Name);
    }

    [Fact]
    public void GetOnDemand_User_OnlyReturnsFlaggedUserContextMenu()
    {
        var registry = BuildRegistryWith(typeof(PremiumOnDemandUserHandler), typeof(ReportUserHandler));

        var onDemand = registry.GetOnDemand(ApplicationCommandType.User);

        Assert.Single(onDemand);
        Assert.Equal("premium-user", onDemand.Single().Name);
    }

    [Fact]
    public void GetOnDemand_Message_OnlyReturnsFlaggedMessageContextMenu()
    {
        var registry = BuildRegistryWith(typeof(PremiumOnDemandMessageHandler), typeof(BookmarkMessageHandler));

        var onDemand = registry.GetOnDemand(ApplicationCommandType.Message);

        Assert.Single(onDemand);
        Assert.Equal("premium-message", onDemand.Single().Name);
    }

    [Fact]
    public void GetOnDemand_InvalidType_Throws()
    {
        var registry = new CommandRegistryBuilder().Build();
        Assert.Throws<ArgumentOutOfRangeException>(() => registry.GetOnDemand((ApplicationCommandType)999));
    }

    [Fact]
    public void IsOnDemand_ReturnsTrueForFlaggedSlashCommand()
    {
        var registry = BuildRegistryWith(typeof(PremiumOnDemandSlashHandler));

        Assert.True(registry.IsOnDemand("premium", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public void IsOnDemand_ReturnsFalseForUnflaggedCommand()
    {
        var registry = BuildRegistryWith(typeof(PingSlashHandler));

        Assert.False(registry.IsOnDemand("ping", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public void IsOnDemand_ReturnsFalseForNonexistentCommand()
    {
        var registry = new CommandRegistryBuilder().Build();

        Assert.False(registry.IsOnDemand("nope", ApplicationCommandType.ChatInput));
        Assert.False(registry.IsOnDemand("nope", ApplicationCommandType.User));
        Assert.False(registry.IsOnDemand("nope", ApplicationCommandType.Message));
    }

    [Fact]
    public void IsOnDemand_DoesNotThrowForBlankName()
    {
        var registry = new CommandRegistryBuilder().Build();

        Assert.False(registry.IsOnDemand(" ", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public void IsOnDemand_DistinguishesByType()
    {
        // Same name "premium" — only the User context menu is marked on-demand.
        var registry = BuildRegistryWith(
            typeof(PremiumPlainSlashHandler),
            typeof(PremiumOnDemandUserContextHandler));

        Assert.False(registry.IsOnDemand("premium", ApplicationCommandType.ChatInput));
        Assert.True(registry.IsOnDemand("premium", ApplicationCommandType.User));
    }

    // ── Slash groups ──

    [Fact]
    public void GetAll_IncludesGroupedSlashCommands()
    {
        var registry = BuildRegistryWith(typeof(NotifyGroupHandler));

        Assert.Contains(registry.GetAll(ApplicationCommandType.ChatInput), c => c.Name == "notify");
    }

    [Fact]
    public void Get_FindsSlashGroupByName()
    {
        var registry = BuildRegistryWith(typeof(NotifyGroupHandler));

        var cmd = registry.Get("notify", ApplicationCommandType.ChatInput);

        Assert.Equal("notify", cmd.Name);
    }

    // ── Test handler classes ──

    private class PingSlashHandler : SlashCommandHandler
    {
        [SlashCommand("ping", "Ping description")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    private class PremiumOnDemandSlashHandler : SlashCommandHandler
    {
        [SlashCommand("premium", "Premium-only slash command")]
        [OnDemand]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    private class PremiumPlainSlashHandler : SlashCommandHandler
    {
        [SlashCommand("premium", "Plain premium slash")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    private class SharedSlashHandler : SlashCommandHandler
    {
        [SlashCommand("shared", "Shared name across types")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }

    private class ReportUserHandler : UserContextMenuHandler
    {
        [ContextMenuCommand("ReportUser")]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    private class PremiumOnDemandUserHandler : UserContextMenuHandler
    {
        [ContextMenuCommand("premium-user")]
        [OnDemand]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    private class PremiumOnDemandUserContextHandler : UserContextMenuHandler
    {
        [ContextMenuCommand("premium")]
        [OnDemand]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    private class SharedUserHandler : UserContextMenuHandler
    {
        [ContextMenuCommand("shared")]
        protected Task ExecuteAsync(IUserCommandContext context) => Task.CompletedTask;
    }

    private class BookmarkMessageHandler : MessageContextMenuHandler
    {
        [ContextMenuCommand("BookmarkMessage")]
        protected Task ExecuteAsync(IMessageCommandContext context) => Task.CompletedTask;
    }

    private class PremiumOnDemandMessageHandler : MessageContextMenuHandler
    {
        [ContextMenuCommand("premium-message")]
        [OnDemand]
        protected Task ExecuteAsync(IMessageCommandContext context) => Task.CompletedTask;
    }

    private class SharedMessageHandler : MessageContextMenuHandler
    {
        [ContextMenuCommand("shared")]
        protected Task ExecuteAsync(IMessageCommandContext context) => Task.CompletedTask;
    }

    private class NotifyGroupHandler : SlashCommandHandler
    {
        [SlashCommand("notify", "Notification group")]
        [SubCommand("enable", "Enable notifications")]
        protected Task EnableAsync(ICommandContext context) => Task.CompletedTask;

        [SlashCommand("notify", "Notification group")]
        [SubCommand("disable", "Disable notifications")]
        protected Task DisableAsync(ICommandContext context) => Task.CompletedTask;
    }
}
