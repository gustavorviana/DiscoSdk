using DiscoSdk.Commands;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// A bound registration scope returned by
/// <see cref="IGuildCommandUpdateFactory.OpenForGuild(Models.Snowflake, bool)"/> or
/// <see cref="ICommandUpdateFactory.OpenForGlobal(bool)"/>. Extends
/// <see cref="ICommandUpdateScopeBuilder"/> with <see cref="ApplyAsync"/> — the commit step
/// that PUTs/POSTs/PATCHes to Discord. Add/remove methods are shadowed (<c>new</c>) so they
/// return <see cref="ICommandUpdateScope"/> directly, preserving fluent chains that end in
/// <see cref="ApplyAsync"/>.
/// </summary>
public interface ICommandUpdateScope : ICommandUpdateScopeBuilder
{
    /// <summary>Queues a slash command (chat input) built via <paramref name="configure"/>.</summary>
    new ICommandUpdateScope AddSlash(Func<SlashCommandBuilder, SlashCommandBuilder> configure);

    /// <summary>Queues a context menu command (User or Message) built via <paramref name="configure"/>.</summary>
    new ICommandUpdateScope AddContextMenu(ContextMenuType type, Func<ContextMenuBuilder, ContextMenuBuilder> configure);

    /// <summary>Queues a pre-built command (any type).</summary>
    new ICommandUpdateScope Add(ApplicationCommand command);

    /// <summary>Pulls an on-demand command from the registry and queues it.</summary>
    new ICommandUpdateScope AddFromCatalog(string name, ApplicationCommandType type);

    /// <summary>Drops a previously-queued command from this scope by name + type.</summary>
    new ICommandUpdateScope Remove(string name, ApplicationCommandType type);

    /// <summary>
    /// Commits the queued commands. Behavior depends on the scope's mode:
    /// <list type="bullet">
    /// <item><description><c>overwrite</c>: PUT bulk — Discord replaces the entire set in the scope.</description></item>
    /// <item><description><c>append</c>: read existing → upsert (POST new, PATCH changed, no-op equal). Never deletes.</description></item>
    /// </list>
    /// </summary>
    Task ApplyAsync(CancellationToken cancellationToken = default);
}
