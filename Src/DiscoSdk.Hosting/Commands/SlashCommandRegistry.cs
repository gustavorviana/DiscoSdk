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
    : ICommandsUpdateWindowModule,
    IApplicationCommandHandler,
    IAutocompleteHandler
{
    private readonly IReadOnlyDictionary<AutocompleteName, AutocompleteInfo> _autocompletes;
    private readonly IReadOnlyDictionary<string, CommandInfo> _commands;
    private readonly IReadOnlyDictionary<string, SlashGroupInfo> _groups;

    public SlashCommandRegistry(IServiceCollection serviceCollection, Assembly[] assemblies)
        : this(serviceCollection, FindSlashCommandHandlers(assemblies))
    {
    }

    internal SlashCommandRegistry(IServiceCollection serviceCollection, IEnumerable<Type> handlerTypes)
    {
        var allCommands = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        var allAutocompletes = new Dictionary<AutocompleteName, AutocompleteInfo>();
        var allGroups = new Dictionary<string, SlashGroupInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in handlerTypes)
        {
            var commandList = CommandInfo.GetAll(type).ToList();

            foreach (var command in commandList)
            {
                if (command.SubCommand != null)
                {
                    if (allCommands.ContainsKey(command.Info.Name))
                        throw new InvalidOperationException(
                            $"Command '{command.Info.Name}' is registered both as a flat command and as a group with subcommands.");

                    if (!allGroups.TryGetValue(command.Info.Name, out var group))
                    {
                        group = new SlashGroupInfo(command.Info);
                        allGroups[command.Info.Name] = group;
                    }

                    group.Add(command);
                }
                else
                {
                    if (allGroups.ContainsKey(command.Info.Name))
                        throw new InvalidOperationException(
                            $"Command '{command.Info.Name}' is registered both as a flat command and as a group with subcommands.");

                    if (!allCommands.TryAdd(command.Info.Name, command))
                        throw new InvalidOperationException(
                            $"Duplicate slash command '{command.Info.Name}' found in type '{type.FullName}'.");
                }
            }

            var autocompletes = AutocompleteInfo.GetAll(type);

            foreach (var (name, autocomplete) in autocompletes)
            {
                if (!allAutocompletes.TryAdd(name, autocomplete))
                    throw new InvalidOperationException(
                        $"Duplicate autocomplete handler for command '{autocomplete.CommandName}', option '{autocomplete.OptionName}'.");
            }

            serviceCollection.AddScoped(type);
        }

        _commands = allCommands.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _autocompletes = allAutocompletes.ToFrozenDictionary();
        _groups = allGroups.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
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

        foreach (var group in _groups.Values)
        {
            var command = group.GetCommandBuilder(_autocompletes.ContainsKey).Build();
            if (group.ParentInfo.IsGuildCommand())
                foreach (var guildId in group.ParentInfo.GuildIds)
                    container.AddGuild(Snowflake.Parse(guildId), command);
            else
                container.AddGlobal(command);
        }
    }

    async Task IDiscordEventHandler<ICommandContext>.HandleAsync(ICommandContext context, IServiceProvider services)
    {
        CommandInfo? command;

        if (context.Subcommand != null)
        {
            if (!_groups.TryGetValue(context.Name, out var group))
                return;

            command = group.FindCommand(context.SubcommandGroup, context.Subcommand);
        }
        else
        {
            _commands.TryGetValue(context.Name, out command);
        }

        if (command == null)
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