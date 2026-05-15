using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Shared transactional unit-of-work for command registration during the SDK's startup
/// window. The auto-register module and the user's <c>CommandsUpdateWindowOpened</c> event
/// handler both receive the same session instance; opening the same target (global or a
/// given guild) returns the <strong>same</strong> <see cref="ICommandUpdateScopeBuilder"/>
/// across participants, so commands accumulate instead of stomping each other.
/// </summary>
/// <remarks>
/// <para>
/// The commit (<see cref="ICommandUpdateScope.ApplyAsync"/>) is <strong>not</strong> exposed
/// on the builder view — participants can only Add / Remove. The SDK orchestrator flushes
/// every accumulated scope once, after every participant has run.
/// </para>
/// <para>
/// <c>overwrite</c> follows last-write-wins: if the module opens a guild scope in append
/// mode and the event handler re-opens it in overwrite mode, the final commit is overwrite.
/// </para>
/// </remarks>
public interface ICommandUpdateSession
{
    /// <summary>Opens (or returns the already-open) builder for the application's global commands.</summary>
    ICommandUpdateScopeBuilder OpenForGlobal(bool overwrite);

    /// <summary>Opens (or returns the already-open) builder for the given <paramref name="guildId"/>.</summary>
    ICommandUpdateScopeBuilder OpenForGuild(Snowflake guildId, bool overwrite);
}
