using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands;

public class AutocompleteSubcommandTests
{
    // ── Key generation tests ──

    [Fact]
    public void AutocompleteName_FlatCommand_GeneratesCorrectKey()
    {
        var name = new AutocompleteName("search", "query");
        Assert.Equal("search::query", name.Name);
    }

    [Fact]
    public void AutocompleteName_WithSubcommand_GeneratesCorrectKey()
    {
        var name = new AutocompleteName("music", "song", subcommand: "play");
        Assert.Equal("music::play::song", name.Name);
    }

    [Fact]
    public void AutocompleteName_WithGroupAndSubcommand_GeneratesCorrectKey()
    {
        var name = new AutocompleteName("music", "song", subcommand: "add", subcommandGroup: "queue");
        Assert.Equal("music::queue::add::song", name.Name);
    }

    [Fact]
    public void AutocompleteName_FlatAndSubcommand_AreNotEqual()
    {
        var flat = new AutocompleteName("cmd", "opt");
        var sub = new AutocompleteName("cmd", "opt", subcommand: "sub");
        Assert.NotEqual(flat, sub);
    }

    [Fact]
    public void AutocompleteName_CaseInsensitiveEquality()
    {
        var a = new AutocompleteName("Music", "Song", subcommand: "Play");
        var b = new AutocompleteName("music", "song", subcommand: "play");
        Assert.Equal(a, b);
    }

    // ── FromContext tests ──

    [Fact]
    public void FromContext_FlatCommand_MatchesFlatKey()
    {
        var context = CreateMockAutocompleteContext("search", "query");
        var fromContext = AutocompleteName.FromContext(context);
        var expected = new AutocompleteName("search", "query");
        Assert.Equal(expected, fromContext);
    }

    [Fact]
    public void FromContext_WithSubcommand_MatchesSubcommandKey()
    {
        var context = CreateMockAutocompleteContext("music", "song", subcommand: "play");
        var fromContext = AutocompleteName.FromContext(context);
        var expected = new AutocompleteName("music", "song", subcommand: "play");
        Assert.Equal(expected, fromContext);
    }

    [Fact]
    public void FromContext_WithGroupAndSubcommand_MatchesGroupKey()
    {
        var context = CreateMockAutocompleteContext("music", "song", subcommand: "add", subcommandGroup: "queue");
        var fromContext = AutocompleteName.FromContext(context);
        var expected = new AutocompleteName("music", "song", subcommand: "add", subcommandGroup: "queue");
        Assert.Equal(expected, fromContext);
    }

    // ── Registry lookup tests ──

    private static readonly Type[] AutocompleteHandlerTypes =
        [typeof(FlatAutocompleteHandler), typeof(SubcmdAutocompleteHandler), typeof(GroupedAutocompleteHandler)];

    private static string? _lastInvokedMethod;
    private static void ResetTracker() => _lastInvokedMethod = null;

    [Fact]
    public async Task HandleAutocompleteAsync_FlatCommand_RoutesToFlatHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, AutocompleteHandlerTypes);

        var sp = BuildServiceProvider();
        var context = CreateMockAutocompleteContext("ac-flat", "query");
        var handler = (IDiscordEventHandler<IAutocompleteContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("FlatAutocompleteHandler.Autocomplete", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAutocompleteAsync_SubcommandOption_RoutesToSubcommandHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, AutocompleteHandlerTypes);

        var sp = BuildServiceProvider();
        var context = CreateMockAutocompleteContext("ac-grouped", "song", subcommand: "play");
        var handler = (IDiscordEventHandler<IAutocompleteContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("SubcmdAutocompleteHandler.Autocomplete", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAutocompleteAsync_GroupedSubcommandOption_RoutesToGroupedHandlerAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, AutocompleteHandlerTypes);

        var sp = BuildServiceProvider();
        var context = CreateMockAutocompleteContext("ac-grouped", "song", subcommand: "add", subcommandGroup: "queue");
        var handler = (IDiscordEventHandler<IAutocompleteContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Equal("GroupedAutocompleteHandler.Autocomplete", _lastInvokedMethod);
    }

    [Fact]
    public async Task HandleAutocompleteAsync_WrongSubcommand_DoesNotRouteAsync()
    {
        ResetTracker();
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, AutocompleteHandlerTypes);

        var sp = BuildServiceProvider();
        // "song" option only exists on subcommand "play", not on a flat lookup
        var context = CreateMockAutocompleteContext("ac-grouped", "song");
        var handler = (IDiscordEventHandler<IAutocompleteContext>)registry;

        await handler.HandleAsync(context, sp);

        Assert.Null(_lastInvokedMethod);
    }

    [Fact]
    public void CommandBuilder_SubcommandWithAutocomplete_SetsAutocompleteFlag()
    {
        var services = new ServiceCollection();
        var registry = new SlashCommandRegistry(services, AutocompleteHandlerTypes);
        var container = new CommandContainer();
        var client = Substitute.For<IDiscordClient>();

        registry.OnCommandsUpdateWindowOpened(client, container);

        // The grouped command should have subcommands with autocomplete-flagged options
        var groupedCommand = container.GlobalCommands.FirstOrDefault(c => c.Name == "ac-grouped");
        Assert.NotNull(groupedCommand);

        // Find the "play" subcommand option
        var playSub = groupedCommand.Options?.FirstOrDefault(o => o.Name == "play" && o.Type == SlashCommandOptionType.SubCommand);
        Assert.NotNull(playSub);

        // The "song" leaf option within "play" should have Autocomplete = true
        var songOption = playSub!.Options?.FirstOrDefault(o => o.Name == "song");
        Assert.NotNull(songOption);
        Assert.True(songOption!.Autocomplete);
    }

    // ── Helpers ──

    private static IAutocompleteContext CreateMockAutocompleteContext(
        string commandName, string focusedOptionName,
        string? subcommand = null, string? subcommandGroup = null)
    {
        var context = Substitute.For<IAutocompleteContext>();
        context.CommandName.Returns(commandName);
        context.Subcommand.Returns(subcommand);
        context.SubcommandGroup.Returns(subcommandGroup);

        var focusedOption = Substitute.For<IAutocompleteFocusedOption>();
        focusedOption.Name.Returns(focusedOptionName);
        focusedOption.Type.Returns(SlashCommandOptionType.String);
        focusedOption.Value.Returns("partial");
        context.FocusedOption.Returns(focusedOption);

        context.Options.Returns(Array.Empty<IAutocompleteOptionValue>());
        return context;
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        _ = new SlashCommandRegistry(services, AutocompleteHandlerTypes);

        var contextProvider = Substitute.For<ISdkContextProvider>();
        contextProvider.GetContext().Returns(Substitute.For<IInteractionContext>());
        services.AddScoped(_ => contextProvider);

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }

    // ── Test handler classes ──

    public class FlatAutocompleteHandler : SlashCommandHandler
    {
        [SlashCommand("ac-flat", "A flat command with autocomplete")]
        [SlashOption(SlashCommandOptionType.String, name: "query", description: "Search query")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;

        [AutocompleteHandler("ac-flat", "query")]
        protected Task AutocompleteAsync(IAutocompleteContext context)
        {
            _lastInvokedMethod = "FlatAutocompleteHandler.Autocomplete";
            return Task.CompletedTask;
        }
    }

    public class SubcmdAutocompleteHandler : SlashCommandHandler
    {
        [SlashCommand("ac-grouped", "A grouped command")]
        [SubCommand("play", "Play a song")]
        [SlashOption(SlashCommandOptionType.String, name: "song", description: "Song name")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;

        [AutocompleteHandler("ac-grouped", "song", Subcommand = "play")]
        protected Task AutocompleteAsync(IAutocompleteContext context)
        {
            _lastInvokedMethod = "SubcmdAutocompleteHandler.Autocomplete";
            return Task.CompletedTask;
        }
    }

    public class GroupedAutocompleteHandler : SlashCommandHandler
    {
        [SlashCommand("ac-grouped", "A grouped command")]
        [SubCommandGroup("queue", "Queue management")]
        [SubCommand("add", "Add to queue")]
        [SlashOption(SlashCommandOptionType.String, name: "song", description: "Song name")]
        protected Task ExecuteAsync(ICommandContext context) => Task.CompletedTask;

        [AutocompleteHandler("ac-grouped", "song", Subcommand = "add", SubcommandGroup = "queue")]
        protected Task AutocompleteAsync(IAutocompleteContext context)
        {
            _lastInvokedMethod = "GroupedAutocompleteHandler.Autocomplete";
            return Task.CompletedTask;
        }
    }
}
