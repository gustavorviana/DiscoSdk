using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Tests.Commands;

public class SlashCommandValidationTests
{
    private static void Scan(params Type[] handlerTypes)
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)handlerTypes).ApplyTo(builder, services);
    }

    [Fact]
    public void Scanner_FlatAndGroupConflict_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Scan(typeof(ValFlatHandler), typeof(ValGroupHandler)));

        Assert.Contains("val-conflict", ex.Message);
    }

    [Fact]
    public void Scanner_GroupAndFlatConflict_ThrowsInvalidOperationException()
    {
        // Reverse order: group first, then flat
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Scan(typeof(ValGroupHandler), typeof(ValFlatHandler)));

        Assert.Contains("val-conflict", ex.Message);
    }

    [Fact]
    public void Scanner_DuplicateSubcommandInGroup_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Scan(typeof(ValDupSubHandler1), typeof(ValDupSubHandler2)));

        Assert.Contains("Duplicate subcommand", ex.Message);
        Assert.Contains("dup-sub", ex.Message);
    }

    [Fact]
    public void Scanner_SubCommandGroupWithoutSubCommand_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Scan(typeof(ValBadGroupHandler)));

        Assert.Contains("[SubCommandGroup]", ex.Message);
        Assert.Contains("[SubCommand]", ex.Message);
    }

    [Fact]
    public void Scanner_ValidHandlers_DoesNotThrow()
    {
        Scan(typeof(GroupTestHandlerSet), typeof(GroupTestHandlerGet), typeof(GroupTestHandlerNotifEnable));
    }
}

// --- Invalid handler types for validation tests ---

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
