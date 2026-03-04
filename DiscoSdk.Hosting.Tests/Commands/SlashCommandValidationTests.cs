using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandValidationTests
{
    [Fact]
    public void Constructor_FlatAndGroupConflict_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new SlashCommandRegistry(services, [typeof(ValFlatHandler), typeof(ValGroupHandler)]));

        Assert.Contains("val-conflict", ex.Message);
    }

    [Fact]
    public void Constructor_GroupAndFlatConflict_ThrowsInvalidOperationException()
    {
        // Reverse order: group first, then flat
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new SlashCommandRegistry(services, [typeof(ValGroupHandler), typeof(ValFlatHandler)]));

        Assert.Contains("val-conflict", ex.Message);
    }

    [Fact]
    public void Constructor_DuplicateSubcommandInGroup_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new SlashCommandRegistry(services, [typeof(ValDupSubHandler1), typeof(ValDupSubHandler2)]));

        Assert.Contains("Duplicate subcommand", ex.Message);
        Assert.Contains("dup-sub", ex.Message);
    }

    [Fact]
    public void Constructor_SubCommandGroupWithoutSubCommand_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new SlashCommandRegistry(services, [typeof(ValBadGroupHandler)]));

        Assert.Contains("[SubCommandGroup]", ex.Message);
        Assert.Contains("[SubCommand]", ex.Message);
    }

    [Fact]
    public void Constructor_ValidHandlers_DoesNotThrow()
    {
        var services = new ServiceCollection();

        var registry = new SlashCommandRegistry(services,
            [typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable)]);

        Assert.NotNull(registry);
    }
}

// --- Invalid handler types for validation tests ---
// These are ONLY used via the internal type-based constructor, never via assembly scanning.

public class ValFlatHandler : SlashCommandHandler
{
    [SlashCommand("val-conflict", "A flat command")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}

public class ValGroupHandler : SlashCommandHandler
{
    [SlashCommand("val-conflict", "A grouped command with same name")]
    [SubCommand("sub", "A subcommand")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}

public class ValDupSubHandler1 : SlashCommandHandler
{
    [SlashCommand("val-dup-parent", "Parent for dup test")]
    [SubCommand("dup-sub", "First subcommand")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}

public class ValDupSubHandler2 : SlashCommandHandler
{
    [SlashCommand("val-dup-parent", "Parent for dup test")]
    [SubCommand("dup-sub", "Duplicate subcommand")]
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}

public class ValBadGroupHandler : SlashCommandHandler
{
    [SlashCommand("val-bad-group", "Command with group but no subcommand")]
    [SubCommandGroup("mygroup", "A group")]
    // Intentionally missing [SubCommand] — this should throw
    protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
}
