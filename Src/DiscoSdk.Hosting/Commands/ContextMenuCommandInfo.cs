using DiscoSdk.Commands;
using DiscoSdk.Hosting.Commands.Callers.Results;
using System.Reflection;

namespace DiscoSdk.Hosting.Commands;

internal class ContextMenuCommandInfo(ContextMenuType type, string name, string[] guildIds, bool isOnDemand, MethodCaller method)
{
    public string Name => name;
    public string[] GuildIds => guildIds;
    public bool IsOnDemand => isOnDemand;
    public Type HandlerType => method.Method.DeclaringType!;
    public ContextMenuType Type => type;

    public bool IsGuildCommand() => GuildIds is { Length: > 0 };

    public async Task ExecuteAsync(object handler, object context, CancellationToken token)
    {
        await method!.ExecuteAsync(handler, [context], token);
    }

    public static IEnumerable<ContextMenuCommandInfo> GetCommands(Type handlerType)
    {
        if (handlerType.IsAbstract || handlerType.IsInterface)
            yield break;

        var menuType = InferMenuType(handlerType);
        if (menuType is null)
            yield break;

        foreach (var method in handlerType.GetMethods(CommandReflection.Flags))
        {
            var attr = method.GetCustomAttribute<ContextMenuCommandAttribute>();
            if (attr is null)
                continue;

            var onDemand = method.GetCustomAttribute<OnDemandAttribute>() != null;
            yield return new ContextMenuCommandInfo(menuType.Value, attr.Name, attr.GuildIds, onDemand, MethodCaller.From(method));
        }
    }

    private static ContextMenuType? InferMenuType(Type handlerType)
    {
        if (typeof(UserContextMenuHandler).IsAssignableFrom(handlerType))
            return ContextMenuType.User;
        if (typeof(MessageContextMenuHandler).IsAssignableFrom(handlerType))
            return ContextMenuType.Message;
        return null;
    }
}
