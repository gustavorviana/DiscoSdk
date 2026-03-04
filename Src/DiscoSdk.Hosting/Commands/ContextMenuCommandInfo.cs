using DiscoSdk.Commands;
using DiscoSdk.Hosting.Commands.Callers.Results;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class ContextMenuCommandInfo
{
    public string Name { get; }
    public string[] GuildIds { get; }
    public Type HandlerType { get; }
    private readonly MethodCaller _method;

    private ContextMenuCommandInfo(string name, string[] guildIds, MethodCaller method)
    {
        Name = name;
        GuildIds = guildIds;
        HandlerType = method.Method.DeclaringType!;
        _method = method;
    }

    public bool IsGuildCommand() => GuildIds is { Length: > 0 };

    public async Task ExecuteAsync(object handler, object context, CancellationToken token)
    {
        await _method.ExecuteAsync(handler, [context], token);
    }

    internal static IEnumerable<ContextMenuCommandInfo> GetUserCommands(Type handlerType)
    {
        if (handlerType.IsAbstract || handlerType.IsInterface || !typeof(UserContextMenuHandler).IsAssignableFrom(handlerType))
            yield break;

        foreach (var method in handlerType.GetMethods(CommandReflection.Flags))
        {
            var attr = method.GetCustomAttribute<UserCommandAttribute>();
            if (attr != null)
                yield return new ContextMenuCommandInfo(attr.Name, attr.GuildIds, MethodCaller.From(method));
        }
    }

    internal static IEnumerable<ContextMenuCommandInfo> GetMessageCommands(Type handlerType)
    {
        if (handlerType.IsAbstract || handlerType.IsInterface || !typeof(MessageContextMenuHandler).IsAssignableFrom(handlerType))
            yield break;

        foreach (var method in handlerType.GetMethods(CommandReflection.Flags))
        {
            var attr = method.GetCustomAttribute<MessageCommandAttribute>();
            if (attr != null)
                yield return new ContextMenuCommandInfo(attr.Name, attr.GuildIds, MethodCaller.From(method));
        }
    }
}
