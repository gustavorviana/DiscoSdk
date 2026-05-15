using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Long-lived event handler for context menu commands (user + message). Resolved from DI with a
/// frozen <see cref="CommandRegistry"/>.
/// </summary>
internal sealed class ContextMenuCommandDispatcher
    : IUserCommandHandler,
    IMessageCommandHandler
{
    private readonly CommandRegistry _registry;

    public ContextMenuCommandDispatcher(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    async Task IDiscordEventHandler<IUserCommandContext>.HandleAsync(IUserCommandContext context, IServiceProvider services)
    {
        var entry = _registry.FindContextMenu(context.Name, ApplicationCommandType.User);
        if (entry is null)
            return;

        var handler = (UserContextMenuHandler)services.GetRequiredService(entry.Info.HandlerType);
        handler.Init(services);
        await entry.Info.ExecuteAsync(handler, context, default);
    }

    async Task IDiscordEventHandler<IMessageCommandContext>.HandleAsync(IMessageCommandContext context, IServiceProvider services)
    {
        var entry = _registry.FindContextMenu(context.Name, ApplicationCommandType.Message);
        if (entry is null)
            return;

        var handler = (MessageContextMenuHandler)services.GetRequiredService(entry.Info.HandlerType);
        handler.Init(services);
        await entry.Info.ExecuteAsync(handler, context, default);
    }
}
