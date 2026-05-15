using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Modules;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Long-lived module that queues the registry's auto-register entries into the shared session
/// when the <c>CommandsUpdateWindow</c> opens. Does not commit — the SDK orchestrator flushes
/// every scope in the session after all participants run. Entries flagged
/// <see cref="DiscoSdk.Commands.OnDemandAttribute">[OnDemand]</see> without <c>GuildIds</c> are
/// skipped (they wait for manual registration via
/// <see cref="IGuildCommandUpdateFactory"/> at runtime).
/// </summary>
internal sealed class CommandAutoRegisterModule : ICommandsUpdateWindowModule
{
    private readonly CommandRegistry _registry;

    public CommandAutoRegisterModule(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    public Task OnCommandsUpdateWindowOpenedAsync(IDiscordClient discordClient, ICommandUpdateSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        QueueInto(session, _registry.EnumerateAutoRegisterSlash());
        QueueInto(session, _registry.EnumerateAutoRegisterContextMenu());
        return Task.CompletedTask;
    }

    private static void QueueInto(
        ICommandUpdateSession session,
        IEnumerable<(ApplicationCommand Command, bool IsOnDemand, IReadOnlyList<Snowflake> GuildIds)> entries)
    {
        foreach (var entry in entries)
        {
            // Globals: !IsOnDemand && no GuildIds
            if (!entry.IsOnDemand && entry.GuildIds.Count == 0)
            {
                session.OpenForGlobal(overwrite: false).Add(entry.Command);
                continue;
            }

            // OnDemand sem GuildIds: dormente — só registra manualmente via IGuildCommandUpdateFactory.
            // OnDemand com GuildIds OU normal com GuildIds: auto-register em cada guild listado.
            foreach (var guildId in entry.GuildIds)
                session.OpenForGuild(guildId, overwrite: false).Add(entry.Command);
        }
    }
}
