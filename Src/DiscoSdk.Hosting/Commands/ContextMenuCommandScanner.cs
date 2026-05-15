using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Reflection-only scanner for context menu handler types (user + message). Discovers
/// <see cref="ContextMenuCommandInfo"/> entries and writes them to a
/// <see cref="CommandRegistryBuilder"/>. After <see cref="ApplyTo"/> the scanner is discarded.
/// </summary>
internal sealed class ContextMenuCommandScanner : ICommandScanner
{
    private readonly IEnumerable<Type> _handlerTypes;

    public ContextMenuCommandScanner(Assembly[] assemblies)
        : this(FindContextMenuHandlers(assemblies))
    {
    }

    public ContextMenuCommandScanner(IEnumerable<Type> handlerTypes)
    {
        ArgumentNullException.ThrowIfNull(handlerTypes);
        _handlerTypes = handlerTypes;
    }

    public void ApplyTo(CommandRegistryBuilder builder, IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        foreach (var type in _handlerTypes)
        {
            foreach (var cmd in ContextMenuCommandInfo.GetUserCommands(type))
                AddContextMenuEntry(builder, cmd, ApplicationCommandType.User, type);

            foreach (var cmd in ContextMenuCommandInfo.GetMessageCommands(type))
                AddContextMenuEntry(builder, cmd, ApplicationCommandType.Message, type);

            services.AddScoped(type);
        }
    }

    private static void AddContextMenuEntry(
        CommandRegistryBuilder builder,
        ContextMenuCommandInfo info,
        ApplicationCommandType type,
        Type declaringType)
    {
        var menuType = type == ApplicationCommandType.User ? ContextMenuType.User : ContextMenuType.Message;
        var built = new ContextMenuBuilder().WithName(info.Name).Build(menuType);
        var guildIds = ParseGuildIds(info.GuildIds);

        try
        {
            builder.AddContextMenu(info, built, type, guildIds, info.IsOnDemand);
        }
        catch (InvalidOperationException ex)
        {
            // Surface declaring type in the message to mirror the diagnostic the old registry produced.
            throw new InvalidOperationException(
                $"Duplicate {(type == ApplicationCommandType.User ? "user" : "message")} command '{info.Name}' found in type '{declaringType.FullName}'.",
                ex);
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

    internal static IEnumerable<Type> FindContextMenuHandlers(Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(GetLoadableTypes)
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                (typeof(UserContextMenuHandler).IsAssignableFrom(t) ||
                 typeof(MessageContextMenuHandler).IsAssignableFrom(t)));
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
