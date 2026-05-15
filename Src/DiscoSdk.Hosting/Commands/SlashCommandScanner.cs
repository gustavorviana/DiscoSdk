using DiscoSdk.Commands;
using DiscoSdk.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Reflection-only scanner for slash command handler types. Discovers <see cref="CommandInfo"/>,
/// groups (<see cref="SlashGroupInfo"/>) and autocompletes, then writes them to a
/// <see cref="CommandRegistryBuilder"/> via <see cref="ApplyTo"/>. After <see cref="ApplyTo"/>
/// returns the scanner is no longer needed.
/// </summary>
internal sealed class SlashCommandScanner : ICommandScanner
{
    private readonly IEnumerable<Type> _handlerTypes;

    public SlashCommandScanner(Assembly[] assemblies)
        : this(FindSlashCommandHandlers(assemblies))
    {
    }

    public SlashCommandScanner(IEnumerable<Type> handlerTypes)
    {
        ArgumentNullException.ThrowIfNull(handlerTypes);
        _handlerTypes = handlerTypes;
    }

    public void ApplyTo(CommandRegistryBuilder builder, IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        // Two-pass: first collect CommandInfos by name (flat vs. group), then build the
        // ApplicationCommands and push them to the builder once per entry.
        var flatByName = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        var groupsByName = new Dictionary<string, SlashGroupInfo>(StringComparer.OrdinalIgnoreCase);
        var groupOnDemandByName = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var pendingAutocompletes = new List<(AutocompleteName Name, AutocompleteInfo Info)>();

        foreach (var type in _handlerTypes)
        {
            foreach (var command in CommandInfo.GetAll(type))
            {
                if (command.SubCommand != null)
                {
                    if (flatByName.ContainsKey(command.Info.Name))
                        throw new InvalidOperationException(
                            $"Command '{command.Info.Name}' is registered both as a flat command and as a group with subcommands.");

                    if (!groupsByName.TryGetValue(command.Info.Name, out var group))
                    {
                        group = new SlashGroupInfo(command.Info);
                        groupsByName[command.Info.Name] = group;
                        groupOnDemandByName[command.Info.Name] = false;
                    }

                    group.Add(command);

                    // The group is on-demand if any sub-command method carries [OnDemand].
                    if (command.IsOnDemand)
                        groupOnDemandByName[command.Info.Name] = true;
                }
                else
                {
                    if (groupsByName.ContainsKey(command.Info.Name))
                        throw new InvalidOperationException(
                            $"Command '{command.Info.Name}' is registered both as a flat command and as a group with subcommands.");

                    if (!flatByName.TryAdd(command.Info.Name, command))
                        throw new InvalidOperationException(
                            $"Duplicate slash command '{command.Info.Name}' found in type '{type.FullName}'.");
                }
            }

            foreach (var (name, autocomplete) in AutocompleteInfo.GetAll(type))
                pendingAutocompletes.Add((name, autocomplete));

            services.AddScoped(type);
        }

        // Autocompletes first — the `HasAutocomplete` callback is used while building the commands.
        foreach (var (name, info) in pendingAutocompletes)
            builder.AddAutocomplete(name, info);

        foreach (var command in flatByName.Values)
        {
            var built = command.GetCommandBuilder(builder.HasAutocomplete).Build();
            var guildIds = ParseGuildIds(command.Info.GuildIds);
            builder.AddSlashFlat(command, built, guildIds, command.IsOnDemand);
        }

        foreach (var group in groupsByName.Values)
        {
            var built = group.GetCommandBuilder(builder.HasAutocomplete).Build();
            var guildIds = ParseGuildIds(group.ParentInfo.GuildIds);
            builder.AddSlashGroup(group, built, guildIds, groupOnDemandByName[group.ParentInfo.Name]);
        }
    }

    private static IReadOnlyList<Snowflake> ParseGuildIds(string[]? rawIds)
    {
        if (rawIds is null || rawIds.Length == 0)
            return Array.Empty<Snowflake>();

        var ids = new Snowflake[rawIds.Length];
        for (var i = 0; i < rawIds.Length; i++)
            ids[i] = Snowflake.Parse(rawIds[i]);
        return ids;
    }

    internal static IEnumerable<Type> FindSlashCommandHandlers(Assembly[] assemblies)
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
