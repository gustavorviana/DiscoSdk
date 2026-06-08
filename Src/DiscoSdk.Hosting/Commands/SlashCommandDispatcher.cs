using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Long-lived event handler for slash commands and AutoCompletes. Resolved from DI with a
/// frozen <see cref="CommandRegistry"/>. Holds no mutable state — every dispatch is a lookup
/// in the registry.
/// </summary>
internal sealed class SlashCommandDispatcher
    : IApplicationCommandHandler,
    IAutoCompleteHandler
{
    private readonly CommandRegistry _registry;

    public SlashCommandDispatcher(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    async Task IDiscordEventHandler<ICommandContext>.HandleAsync(ICommandContext context, IServiceProvider services)
    {
        var entry = _registry.FindSlash(context.Name);
        if (entry is null)
            return;

        CommandInfo? command;
        if (context.Subcommand != null)
            command = entry.Group?.FindCommand(context.SubcommandGroup, context.Subcommand);
        else
            command = entry.Flat;

        if (command is null)
            return;

        await command.ExecuteAsync(context, services, default);
    }

    async Task IDiscordEventHandler<IAutoCompleteContext>.HandleAsync(IAutoCompleteContext context, IServiceProvider services)
    {
        var name = AutoCompleteName.FromContext(context);
        var AutoComplete = _registry.FindAutoComplete(name);
        if (AutoComplete is null)
            return;

        await AutoComplete.ExecuteAsync(services, context, default);
    }
}
