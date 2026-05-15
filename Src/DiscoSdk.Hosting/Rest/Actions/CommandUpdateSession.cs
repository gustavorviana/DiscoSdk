using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Internal implementation of <see cref="ICommandUpdateSession"/>. Memoizes scopes by target so
/// multiple participants (auto-register module + user event handler) accumulate into the same
/// builder. Mode is last-write-wins on re-open. <see cref="ApplyAllAsync"/> commits every
/// accumulated scope once and is the only path that saves to Discord.
/// </summary>
internal sealed class CommandUpdateSession : ICommandUpdateSession
{
    private readonly CommandUpdateFactory _factory;
    private CommandUpdateScope? _global;
    private readonly Dictionary<Snowflake, CommandUpdateScope> _byGuild = new();

    public CommandUpdateSession(CommandUpdateFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    public ICommandUpdateScopeBuilder OpenForGlobal(bool overwrite)
    {
        if (_global is null)
            _global = (CommandUpdateScope)_factory.OpenForGlobal(overwrite);
        else
            _global.SetMode(overwrite);
        return _global;
    }

    public ICommandUpdateScopeBuilder OpenForGuild(Snowflake guildId, bool overwrite)
    {
        if (!_byGuild.TryGetValue(guildId, out var scope))
        {
            scope = (CommandUpdateScope)_factory.OpenForGuild(guildId, overwrite);
            _byGuild[guildId] = scope;
        }
        else
        {
            scope.SetMode(overwrite);
        }
        return scope;
    }

    /// <summary>Commits every accumulated scope once. Called by <see cref="DiscordClient"/>.</summary>
    internal async Task ApplyAllAsync(CancellationToken ct = default)
    {
        if (_global is not null)
            await _global.ApplyAsync(ct);

        foreach (var scope in _byGuild.Values)
            await scope.ApplyAsync(ct);
    }
}
