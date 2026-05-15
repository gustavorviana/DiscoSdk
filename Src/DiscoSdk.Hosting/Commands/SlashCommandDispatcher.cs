using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Long-lived event handler for slash commands and autocompletes. Resolved from DI with a
/// frozen <see cref="CommandRegistry"/>. Holds no mutable state — every dispatch is a lookup
/// in the registry.
/// </summary>
internal sealed class SlashCommandDispatcher
    : IApplicationCommandHandler,
    IAutocompleteHandler
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

    async Task IDiscordEventHandler<IAutocompleteContext>.HandleAsync(IAutocompleteContext context, IServiceProvider services)
    {
        var name = AutocompleteName.FromContext(context);
        var autocomplete = _registry.FindAutocomplete(name);
        if (autocomplete is null)
            return;

        await autocomplete.ExecuteAsync(services, context, default);
    }
}
