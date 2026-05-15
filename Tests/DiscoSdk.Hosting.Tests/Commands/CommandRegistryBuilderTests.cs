using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Tests.Commands;

public class CommandRegistryBuilderTests
{
    private static CommandRegistryBuilder PopulatedSlashBuilder()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)new[] { typeof(SampleSlashHandler) }).ApplyTo(builder, services);
        return builder;
    }

    [Fact]
    public void Build_ReturnsSameInstanceAcrossCalls()
    {
        var builder = PopulatedSlashBuilder();

        var first = builder.Build();
        var second = builder.Build();

        Assert.Same(first, second);
    }

    [Fact]
    public void AddAfterBuild_Throws()
    {
        var builder = PopulatedSlashBuilder();
        _ = builder.Build();

        var info = new AutocompleteName("any", "x", null, null);
        Assert.Throws<InvalidOperationException>(() =>
            builder.AddAutocomplete(info, null!));
    }

    [Fact]
    public void Build_OnEmptyBuilder_ProducesUsableEmptyRegistry()
    {
        var registry = new CommandRegistryBuilder().Build();

        Assert.Empty(registry.GetAll(ApplicationCommandType.ChatInput));
        Assert.Empty(registry.GetAll(ApplicationCommandType.User));
        Assert.Empty(registry.GetAll(ApplicationCommandType.Message));
        Assert.Empty(registry.GetOnDemand(ApplicationCommandType.ChatInput));
        Assert.False(registry.TryGet("any", ApplicationCommandType.ChatInput, out _));
    }

    [Fact]
    public void Build_PreservesPopulatedSlashCommands()
    {
        var builder = PopulatedSlashBuilder();

        var registry = builder.Build();

        Assert.Contains(registry.GetAll(ApplicationCommandType.ChatInput), c => c.Name == "sample-slash");
    }

    private class SampleSlashHandler : SlashCommandHandler
    {
        [SlashCommand("sample-slash", "Sample slash command")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;
    }
}
