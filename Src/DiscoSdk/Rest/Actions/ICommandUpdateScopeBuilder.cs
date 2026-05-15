using DiscoSdk.Commands;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Add/remove-only view of a command update scope. Surfaces every mutation operation
/// (queue a command, drop a command) but <strong>not</strong> the commit step. Used inside
/// <see cref="ICommandUpdateSession"/> so multiple participants (the auto-register module,
/// user event handlers) accumulate ops into the same shared scope; the commit happens once
/// at the end of the startup window, controlled by the orchestrator.
/// </summary>
public interface ICommandUpdateScopeBuilder
{
    /// <summary>Queues a slash command (chat input) built via <paramref name="configure"/>.</summary>
    ICommandUpdateScopeBuilder AddSlash(Func<SlashCommandBuilder, SlashCommandBuilder> configure);

    /// <summary>
    /// Queues a context menu command (User or Message) built via <paramref name="configure"/>.
    /// </summary>
    ICommandUpdateScopeBuilder AddContextMenu(ContextMenuType type, Func<ContextMenuBuilder, ContextMenuBuilder> configure);

    /// <summary>Queues a pre-built command (any type).</summary>
    ICommandUpdateScopeBuilder Add(ApplicationCommand command);

    /// <summary>
    /// Pulls a command from <see cref="ICommandRegistry"/> by its <paramref name="name"/> and
    /// <paramref name="type"/> and queues it. The lookup is restricted to commands flagged
    /// with <see cref="OnDemandAttribute">[OnDemand]</see>.
    /// </summary>
    ICommandUpdateScopeBuilder AddFromCatalog(string name, ApplicationCommandType type);

    /// <summary>
    /// Drops a previously-queued command from this scope by <paramref name="name"/> and
    /// <paramref name="type"/>. No-op if the command isn't queued. Useful when the auto-register
    /// module added a command but the calling code wants to opt out of it before the commit.
    /// </summary>
    ICommandUpdateScopeBuilder Remove(string name, ApplicationCommandType type);
}
