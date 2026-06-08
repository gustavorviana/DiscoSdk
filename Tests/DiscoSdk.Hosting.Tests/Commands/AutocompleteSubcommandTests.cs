using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Hosting.Tests.Commands.TestHelpers;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class AutoCompleteSubcommandTests
{
    // ── Key generation tests ──

    [Fact]
    public void AutoCompleteName_FlatCommand_GeneratesCorrectKey()
    {
        var name = new AutoCompleteName("search", "query");
        Assert.Equal("search::query", name.Name);
    }

    [Fact]
    public void AutoCompleteName_WithSubcommand_GeneratesCorrectKey()
    {
        var name = new AutoCompleteName("music", "song", subcommand: "play");
        Assert.Equal("music::play::song", name.Name);
    }

    [Fact]
    public void AutoCompleteName_WithGroupAndSubcommand_GeneratesCorrectKey()
    {
        var name = new AutoCompleteName("music", "song", subcommand: "add", subcommandGroup: "queue");
        Assert.Equal("music::queue::add::song", name.Name);
    }

    [Fact]
    public void AutoCompleteName_FlatAndSubcommand_AreNotEqual()
    {
        var flat = new AutoCompleteName("cmd", "opt");
        var sub = new AutoCompleteName("cmd", "opt", subcommand: "sub");
        Assert.NotEqual(flat, sub);
    }

    [Fact]
    public void AutoCompleteName_CaseInsensitiveEquality()
    {
        var a = new AutoCompleteName("Music", "Song", subcommand: "Play");
        var b = new AutoCompleteName("music", "song", subcommand: "play");
        Assert.Equal(a, b);
    }

    // ── FromContext tests ──

    [Fact]
    public void FromContext_FlatCommand_MatchesFlatKey()
    {
        var context = CreateMockAutoCompleteContext("search", "query");
        var fromContext = AutoCompleteName.FromContext(context);
        var expected = new AutoCompleteName("search", "query");
        Assert.Equal(expected, fromContext);
    }

    [Fact]
    public void FromContext_WithSubcommand_MatchesSubcommandKey()
    {
        var context = CreateMockAutoCompleteContext("music", "song", subcommand: "play");
        var fromContext = AutoCompleteName.FromContext(context);
        var expected = new AutoCompleteName("music", "song", subcommand: "play");
        Assert.Equal(expected, fromContext);
    }

    [Fact]
    public void FromContext_WithGroupAndSubcommand_MatchesGroupKey()
    {
        var context = CreateMockAutoCompleteContext("music", "song", subcommand: "add", subcommandGroup: "queue");
        var fromContext = AutoCompleteName.FromContext(context);
        var expected = new AutoCompleteName("music", "song", subcommand: "add", subcommandGroup: "queue");
        Assert.Equal(expected, fromContext);
    }

    // ── Registry lookup tests ──

    private static readonly Type[] AutoCompleteHandlerTypes =
        [typeof(FlatAutoCompleteHandler), typeof(SubcmdAutoCompleteHandler), typeof(GroupedAutoCompleteHandler)];

    private static string? _lastInvokedMethod;
    private static void ResetTracker() => _lastInvokedMethod = null;

    private static (SlashCommandDispatcher dispatcher, CommandAutoRegisterModule module, IServiceProvider services) BuildHarness()
    {
        var services = new ServiceCollection();
        var builder = new CommandRegistryBuilder();
        new SlashCommandScanner((IEnumerable<Type>)AutoCompleteHandlerTypes).ApplyTo(builder, services);
        var registry = builder.Build();

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns(Substitute.For<IInteractionContext>());
        services.AddScoped(_ => contextProvider);

        var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;
        return (new SlashCommandDispatcher(registry), new CommandAutoRegisterModule(registry), sp);
    }

    [Fact]
    public async Task HandleAutoCompleteAsync_FlatCommand_RoutesToFlatHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness();
        var context = CreateMockAutoCompleteContext("ac-flat", "query");
        var handler = (IDiscordEventHandler<IAutoCompleteContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("FlatAutoCompleteHandler.AutoComplete", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAutoCompleteAsync_SubcommandOption_RoutesToSubcommandHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness();
        var context = CreateMockAutoCompleteContext("ac-grouped", "song", subcommand: "play");
        var handler = (IDiscordEventHandler<IAutoCompleteContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("SubcmdAutoCompleteHandler.AutoComplete", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAutoCompleteAsync_GroupedSubcommandOption_RoutesToGroupedHandlerAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness();
        var context = CreateMockAutoCompleteContext("ac-grouped", "song", subcommand: "add", subcommandGroup: "queue");
        var handler = (IDiscordEventHandler<IAutoCompleteContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Equal("GroupedAutoCompleteHandler.AutoComplete", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAutoCompleteAsync_WrongSubcommand_DoesNotRouteAsync()
    {
        ResetTracker();
        var (dispatcher, _, sp) = BuildHarness();
        // "song" option only exists on subcommand "play", not on a flat lookup
        var context = CreateMockAutoCompleteContext("ac-grouped", "song");
        var handler = (IDiscordEventHandler<IAutoCompleteContext>)dispatcher;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    [Fact]
    public async Task CommandBuilder_SubcommandWithAutoComplete_SetsAutoCompleteFlagAsync()
    {
        var (_, module, _) = BuildHarness();
        var factory = new CapturingCommandUpdateFactory();
        var client = Substitute.For<IDiscordClient>();

        await module.OnCommandsUpdateWindowOpenedAsync(client, factory);

        // The grouped command should have subcommands with AutoComplete-flagged options
        var groupedCommand = factory.GlobalCommands.FirstOrDefault(c => c.Name == "ac-grouped");
        Assert.NotNull(groupedCommand);

        // Find the "play" subcommand option
        var playSub = groupedCommand.Options?.FirstOrDefault(o => o.Name == "play" && o.Type == SlashCommandOptionType.SubCommand);
        Assert.NotNull(playSub);

        // The "song" leaf option within "play" should have AutoComplete = true
        var songOption = playSub!.Options?.FirstOrDefault(o => o.Name == "song");
        Assert.NotNull(songOption);
        Assert.True(songOption!.AutoComplete);
    }

    // ── Helpers ──

    private static IAutoCompleteContext CreateMockAutoCompleteContext(
        string commandName, string focusedOptionName,
        string? subcommand = null, string? subcommandGroup = null)
    {
        var context = Substitute.For<IAutoCompleteContext>();
        context.CommandName.Returns(commandName);
        context.Subcommand.Returns(subcommand);
        context.SubcommandGroup.Returns(subcommandGroup);

        var focusedOption = Substitute.For<IAutoCompleteFocusedOption>();
        focusedOption.Name.Returns(focusedOptionName);
        focusedOption.Type.Returns(SlashCommandOptionType.String);
        focusedOption.Value.Returns("partial");
        context.FocusedOption.Returns(focusedOption);

        context.Options.Returns(Array.Empty<IAutoCompleteOptionValue>());
        return context;
    }

    // ── Test handler classes ──

    public class FlatAutoCompleteHandler : SlashCommandHandler
    {
        [SlashCommand("ac-flat", "A flat command with AutoComplete")]
        [SlashOption(SlashCommandOptionType.String, name: "query", description: "Search query")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;

        [AutoCompleteHandler("ac-flat", "query")]
        protected Task AutoCompleteAsync(IAutoCompleteContext context)
        {
            _lastInvokedMethod = "FlatAutoCompleteHandler.AutoComplete";
            return Task.CompletedTask;
        }
    }

    public class SubcmdAutoCompleteHandler : SlashCommandHandler
    {
        [SlashCommand("ac-grouped", "A grouped command")]
        [SubCommand("play", "Play a song")]
        [SlashOption(SlashCommandOptionType.String, name: "song", description: "Song name")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;

        [AutoCompleteHandler("ac-grouped", "song", Subcommand = "play")]
        protected Task AutoCompleteAsync(IAutoCompleteContext context)
        {
            _lastInvokedMethod = "SubcmdAutoCompleteHandler.AutoComplete";
            return Task.CompletedTask;
        }
    }

    public class GroupedAutoCompleteHandler : SlashCommandHandler
    {
        [SlashCommand("ac-grouped", "A grouped command")]
        [SubCommandGroup("queue", "Queue management")]
        [SubCommand("add", "Add to queue")]
        [SlashOption(SlashCommandOptionType.String, name: "song", description: "Song name")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;

        [AutoCompleteHandler("ac-grouped", "song", Subcommand = "add", SubcommandGroup = "queue")]
        protected Task AutoCompleteAsync(IAutoCompleteContext context)
        {
            _lastInvokedMethod = "GroupedAutoCompleteHandler.AutoComplete";
            return Task.CompletedTask;
        }
    }
}
