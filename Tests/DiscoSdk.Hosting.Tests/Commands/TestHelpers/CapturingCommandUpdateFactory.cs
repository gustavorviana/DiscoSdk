using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Tests.Commands.TestHelpers;

/// <summary>
/// Test double for <see cref="ICommandUpdateFactory"/> + <see cref="ICommandUpdateSession"/>.
/// Captures every command added to every scope (global or per-guild) so tests can assert
/// against the accumulated state without hitting the network. Session ops are memoized by
/// target — same target returns the same scope across opens.
/// </summary>
internal sealed class CapturingCommandUpdateFactory : ICommandUpdateFactory, ICommandUpdateSession
{
    public List<ApplicationCommand> GlobalCommands { get; } = new();
    public Dictionary<Snowflake, List<ApplicationCommand>> GuildCommands { get; } = new();
    public List<(bool Overwrite, Snowflake? GuildId)> OpenedScopes { get; } = new();
    public int AppliedScopes { get; private set; }

    private Scope? _globalScope;
    private readonly Dictionary<Snowflake, Scope> _guildScopes = new();

    // ── ICommandUpdateFactory: fresh scope per call (runtime use) ──
    public ICommandUpdateScope OpenForGlobal(bool overwrite)
    {
        OpenedScopes.Add((overwrite, null));
        return new Scope(GlobalCommands, () => AppliedScopes++);
    }

    public ICommandUpdateScope OpenForGuild(Snowflake guildId, bool overwrite)
    {
        OpenedScopes.Add((overwrite, guildId));
        if (!GuildCommands.TryGetValue(guildId, out var list))
            GuildCommands[guildId] = list = new List<ApplicationCommand>();
        return new Scope(list, () => AppliedScopes++);
    }

    // ── ICommandUpdateSession: memoized + builder view (startup use) ──
    ICommandUpdateScopeBuilder ICommandUpdateSession.OpenForGlobal(bool overwrite)
    {
        OpenedScopes.Add((overwrite, null));
        return _globalScope ??= new Scope(GlobalCommands, () => AppliedScopes++);
    }

    ICommandUpdateScopeBuilder ICommandUpdateSession.OpenForGuild(Snowflake guildId, bool overwrite)
    {
        OpenedScopes.Add((overwrite, guildId));
        if (_guildScopes.TryGetValue(guildId, out var existing)) return existing;
        if (!GuildCommands.TryGetValue(guildId, out var list))
            GuildCommands[guildId] = list = new List<ApplicationCommand>();
        var scope = new Scope(list, () => AppliedScopes++);
        _guildScopes[guildId] = scope;
        return scope;
    }

    private sealed class Scope : ICommandUpdateScope
    {
        private readonly List<ApplicationCommand> _list;
        private readonly Action _onApply;

        public Scope(List<ApplicationCommand> list, Action onApply)
        {
            _list = list;
            _onApply = onApply;
        }

        public ICommandUpdateScope AddSlash(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
        {
            _list.Add(configure(new SlashCommandBuilder()).Build());
            return this;
        }

        public ICommandUpdateScope AddContextMenu(ContextMenuType type, Func<ContextMenuBuilder, ContextMenuBuilder> configure)
        {
            _list.Add(configure(new ContextMenuBuilder()).Build(type));
            return this;
        }

        public ICommandUpdateScope Add(ApplicationCommand command)
        {
            _list.Add(command);
            return this;
        }

        public ICommandUpdateScope AddFromCatalog(string name, ApplicationCommandType type)
            => throw new NotSupportedException(
                "CapturingCommandUpdateFactory does not back a catalog; assert via Global/Guild captures instead.");

        public ICommandUpdateScope Remove(string name, ApplicationCommandType type)
        {
            _list.RemoveAll(c =>
                (c.Type ?? ApplicationCommandType.ChatInput) == type &&
                StringComparer.OrdinalIgnoreCase.Equals(c.Name, name));
            return this;
        }

        ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.AddSlash(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
            => AddSlash(configure);
        ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.AddContextMenu(ContextMenuType type, Func<ContextMenuBuilder, ContextMenuBuilder> configure)
            => AddContextMenu(type, configure);
        ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.Add(ApplicationCommand command) => Add(command);
        ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.AddFromCatalog(string name, ApplicationCommandType type) => AddFromCatalog(name, type);
        ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.Remove(string name, ApplicationCommandType type) => Remove(name, type);

        public Task ApplyAsync(CancellationToken cancellationToken = default)
        {
            _onApply();
            return Task.CompletedTask;
        }
    }
}
