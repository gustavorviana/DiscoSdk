using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models;
using DiscoSdk.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class SlashCommandRegistry
    : ILifetimeDiscoModule,
    IApplicationCommandHandler,
    IAutocompleteHandler
{
    private readonly IReadOnlyDictionary<AutocompleteName, AutocompleteInfo> _autocompletes;
    private readonly IReadOnlyDictionary<string, CommandInfo> _commands;

    public SlashCommandRegistry(IServiceCollection serviceCollection, Assembly[] assemblies)
    {
        var foundTypes = new HashSet<Type>();
        var seenCommandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenAutocompletes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in FindSlashCommandHandlers(assemblies))
        {
            var commandList = CommandInfo.GetAll(type).ToList();

            foreach (var command in commandList)
                if (!seenCommandNames.Add(command.Info.Name))
                    throw new InvalidOperationException(
                        $"Duplicate slash command '{command.Info.Name}' found in type '{type.FullName}'.");

            var commands = commandList.ToFrozenDictionary(x => x.Info.Name, x => x, StringComparer.OrdinalIgnoreCase);
            var autocompletes = AutocompleteInfo.GetAll(type);

            foreach (var autocomplete in autocompletes.Values)
            {
                var key = $"{autocomplete.CommandName}::{autocomplete.OptionName}";
                if (!seenAutocompletes.Add(key))
                    throw new InvalidOperationException(
                        $"Duplicate autocomplete handler for command '{autocomplete.CommandName}', option '{autocomplete.OptionName}'.");
            }

            serviceCollection.AddScoped(type);

            _autocompletes = autocompletes;
            _commands = commands;
        }
    }

    Task ILifetimeDiscoModule.OnPreInitializeAsync(IDiscordClient discordClient)
    {
        return Task.CompletedTask;
    }


    public void OnCommandsUpdateWindowOpened(IDiscordClient discordClient, CommandContainer container)
    {
        foreach (var item in _commands.Values)
        {
            var command = item.GetCommandBuilder(_autocompletes.ContainsKey).Build();
            if (item.Info.IsGuildCommand())
                foreach (var guildId in item.Info.GuildIds)
                    container.AddGuild(Snowflake.Parse(guildId), command);
            else
                container.AddGlobal(command);
        }
    }

    Task ILifetimeDiscoModule.OnGatewayReadyAsync(IDiscordClient discordClient)
    {
        return Task.CompletedTask;
    }

    Task ILifetimeDiscoModule.OnShutdownAsync(IDiscordClient discordClient)
    {
        return Task.CompletedTask;
    }

    async Task IDiscordEventHandler<ICommandContext>.HandleAsync(ICommandContext context, IServiceProvider services)
    {
        if (!_commands.TryGetValue(context.Name, out var command))
            return;

        await command.ExecuteAsync(context, services, default);
    }

    async Task IDiscordEventHandler<IAutocompleteContext>.HandleAsync(IAutocompleteContext context, IServiceProvider services)
    {
        var name = AutocompleteName.FromContext(context);
        if (!_autocompletes.TryGetValue(name, out var autocomplete))
            return;

        await autocomplete.ExecuteAsync(services, context, default);
    }

    private static IEnumerable<Type> FindSlashCommandHandlers(Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(GetLoadableTypes)
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                typeof(SlashCommandHandler).IsAssignableFrom(t));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }
}