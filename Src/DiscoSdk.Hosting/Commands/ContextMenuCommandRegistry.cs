using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models;
using DiscoSdk.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class ContextMenuCommandRegistry
    : ICommandsUpdateWindowModule,
    IUserCommandHandler,
    IMessageCommandHandler
{
    private readonly IReadOnlyDictionary<string, ContextMenuCommandInfo> _userCommands;
    private readonly IReadOnlyDictionary<string, ContextMenuCommandInfo> _messageCommands;

    public ContextMenuCommandRegistry(IServiceCollection serviceCollection, Assembly[] assemblies)
        : this(serviceCollection, FindContextMenuHandlers(assemblies))
    {
    }

    internal ContextMenuCommandRegistry(IServiceCollection serviceCollection, IEnumerable<Type> handlerTypes)
    {
        var userCommands = new Dictionary<string, ContextMenuCommandInfo>(StringComparer.OrdinalIgnoreCase);
        var messageCommands = new Dictionary<string, ContextMenuCommandInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in handlerTypes)
        {
            foreach (var cmd in ContextMenuCommandInfo.GetUserCommands(type))
            {
                if (!userCommands.TryAdd(cmd.Name, cmd))
                    throw new InvalidOperationException(
                        $"Duplicate user command '{cmd.Name}' found in type '{type.FullName}'.");
            }

            foreach (var cmd in ContextMenuCommandInfo.GetMessageCommands(type))
            {
                if (!messageCommands.TryAdd(cmd.Name, cmd))
                    throw new InvalidOperationException(
                        $"Duplicate message command '{cmd.Name}' found in type '{type.FullName}'.");
            }

            serviceCollection.AddScoped(type);
        }

        _userCommands = userCommands.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _messageCommands = messageCommands.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public void OnCommandsUpdateWindowOpened(IDiscordClient discordClient, CommandContainer container)
    {
        foreach (var cmd in _userCommands.Values)
        {
            var command = new UserCommandBuilder().WithName(cmd.Name).Build();
            if (cmd.IsGuildCommand())
                foreach (var guildId in cmd.GuildIds)
                    container.AddGuild(Snowflake.Parse(guildId), command);
            else
                container.AddGlobal(command);
        }

        foreach (var cmd in _messageCommands.Values)
        {
            var command = new MessageCommandBuilder().WithName(cmd.Name).Build();
            if (cmd.IsGuildCommand())
                foreach (var guildId in cmd.GuildIds)
                    container.AddGuild(Snowflake.Parse(guildId), command);
            else
                container.AddGlobal(command);
        }
    }

    async Task IDiscordEventHandler<IUserCommandContext>.HandleAsync(IUserCommandContext context, IServiceProvider services)
    {
        if (!_userCommands.TryGetValue(context.Name, out var command))
            return;

        var handler = (UserContextMenuHandler)services.GetRequiredService(command.HandlerType);
        handler.Init(services);
        await command.ExecuteAsync(handler, context, default);
    }

    async Task IDiscordEventHandler<IMessageCommandContext>.HandleAsync(IMessageCommandContext context, IServiceProvider services)
    {
        if (!_messageCommands.TryGetValue(context.Name, out var command))
            return;

        var handler = (MessageContextMenuHandler)services.GetRequiredService(command.HandlerType);
        handler.Init(services);
        await command.ExecuteAsync(handler, context, default);
    }

    private static IEnumerable<Type> FindContextMenuHandlers(Assembly[] assemblies)
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
