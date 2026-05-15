using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using System.Collections.Frozen;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Immutable read-side of the command storage produced by <see cref="CommandRegistryBuilder"/>.
/// Holds frozen collections for the three lookup buckets (slash flat/group, context menu,
/// autocomplete) plus the two on-demand HashSets. Per-type snapshots for <see cref="GetAll"/>
/// and <see cref="GetOnDemand"/> are pre-computed at construction so callers pay zero allocation
/// to enumerate.
/// </summary>
internal sealed class CommandRegistry : ICommandRegistry
{
    private readonly FrozenDictionary<string, SlashEntry> _slash;
    private readonly FrozenDictionary<AutocompleteName, AutocompleteInfo> _autocompletes;
    private readonly FrozenDictionary<(string Name, ApplicationCommandType Type), ContextMenuEntry> _contextMenu;
    private readonly FrozenSet<string> _onDemandSlash;
    private readonly FrozenSet<(string Name, ApplicationCommandType Type)> _onDemandContextMenu;

    // Pre-computed enumeration snapshots — entries são imutáveis após Build, então cacheamos.
    private readonly ApplicationCommand[] _slashSnapshot;
    private readonly ApplicationCommand[] _userCtxSnapshot;
    private readonly ApplicationCommand[] _messageCtxSnapshot;
    private readonly ApplicationCommand[] _onDemandSlashSnapshot;
    private readonly ApplicationCommand[] _onDemandUserCtxSnapshot;
    private readonly ApplicationCommand[] _onDemandMessageCtxSnapshot;

    internal CommandRegistry(
        FrozenDictionary<string, SlashEntry> slash,
        FrozenDictionary<AutocompleteName, AutocompleteInfo> autocompletes,
        FrozenDictionary<(string Name, ApplicationCommandType Type), ContextMenuEntry> contextMenu,
        FrozenSet<string> onDemandSlash,
        FrozenSet<(string Name, ApplicationCommandType Type)> onDemandContextMenu)
    {
        _slash = slash;
        _autocompletes = autocompletes;
        _contextMenu = contextMenu;
        _onDemandSlash = onDemandSlash;
        _onDemandContextMenu = onDemandContextMenu;

        _slashSnapshot = _slash.Values.Select(e => e.Command).ToArray();
        _userCtxSnapshot = _contextMenu.Values
            .Where(e => e.Type == ApplicationCommandType.User)
            .Select(e => e.Command).ToArray();
        _messageCtxSnapshot = _contextMenu.Values
            .Where(e => e.Type == ApplicationCommandType.Message)
            .Select(e => e.Command).ToArray();

        _onDemandSlashSnapshot = _onDemandSlash
            .Where(_slash.ContainsKey)
            .Select(name => _slash[name].Command).ToArray();
        _onDemandUserCtxSnapshot = _onDemandContextMenu
            .Where(key => key.Type == ApplicationCommandType.User && _contextMenu.ContainsKey(key))
            .Select(key => _contextMenu[key].Command).ToArray();
        _onDemandMessageCtxSnapshot = _onDemandContextMenu
            .Where(key => key.Type == ApplicationCommandType.Message && _contextMenu.ContainsKey(key))
            .Select(key => _contextMenu[key].Command).ToArray();
    }

    // ── ICommandRegistry: lookups ──

    public ApplicationCommand Get(string name, ApplicationCommandType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null, empty, or whitespace.", nameof(name));

        if (TryGet(name, type, out var command))
            return command;

        throw new KeyNotFoundException(
            $"No command named '{name}' of type {type} was found in the registry.");
    }

    public bool TryGet(string name, ApplicationCommandType type, out ApplicationCommand command)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            command = null!;
            return false;
        }

        if (type == ApplicationCommandType.ChatInput)
        {
            if (_slash.TryGetValue(name, out var slash))
            {
                command = slash.Command;
                return true;
            }
        }
        else
        {
            if (_contextMenu.TryGetValue((name, type), out var ctx))
            {
                command = ctx.Command;
                return true;
            }
        }

        command = null!;
        return false;
    }

    // ── ICommandRegistry: views por tipo (zero alocação por chamada) ──

    public IReadOnlyCollection<ApplicationCommand> GetAll(ApplicationCommandType type) => type switch
    {
        ApplicationCommandType.ChatInput => _slashSnapshot,
        ApplicationCommandType.User => _userCtxSnapshot,
        ApplicationCommandType.Message => _messageCtxSnapshot,
        _ => throw ApplicationCommandTypeGuard.Unsupported(type, nameof(type)),
    };

    public IReadOnlyCollection<ApplicationCommand> GetOnDemand(ApplicationCommandType type) => type switch
    {
        ApplicationCommandType.ChatInput => _onDemandSlashSnapshot,
        ApplicationCommandType.User => _onDemandUserCtxSnapshot,
        ApplicationCommandType.Message => _onDemandMessageCtxSnapshot,
        _ => throw ApplicationCommandTypeGuard.Unsupported(type, nameof(type)),
    };

    public bool IsOnDemand(string name, ApplicationCommandType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        return type == ApplicationCommandType.ChatInput
            ? _onDemandSlash.Contains(name)
            : _onDemandContextMenu.Contains((name, type));
    }

    // ── Internal: dispatch lookups ──

    internal SlashEntry? FindSlash(string name)
        => string.IsNullOrEmpty(name)
            ? null
            : _slash.TryGetValue(name, out var entry) ? entry : null;

    internal AutocompleteInfo? FindAutocomplete(AutocompleteName name)
        => _autocompletes.TryGetValue(name, out var info) ? info : null;

    internal ContextMenuEntry? FindContextMenu(string name, ApplicationCommandType type)
        => string.IsNullOrEmpty(name)
            ? null
            : _contextMenu.TryGetValue((name, type), out var entry) ? entry : null;

    // ── Internal: enumeração das entries pro auto-register module ──

    internal IEnumerable<(ApplicationCommand Command, bool IsOnDemand, IReadOnlyList<Snowflake> GuildIds)> EnumerateAutoRegisterSlash()
        => _slash.Values.Select(e => (e.Command, IsOnDemand: _onDemandSlash.Contains(e.Name), e.GuildIds));

    internal IEnumerable<(ApplicationCommand Command, bool IsOnDemand, IReadOnlyList<Snowflake> GuildIds)> EnumerateAutoRegisterContextMenu()
        => _contextMenu.Values.Select(e => (e.Command, IsOnDemand: _onDemandContextMenu.Contains((e.Name, e.Type)), e.GuildIds));
}

internal sealed class ContextMenuKeyComparer : IEqualityComparer<(string Name, ApplicationCommandType Type)>
{
    public static readonly ContextMenuKeyComparer Instance = new();

    public bool Equals((string Name, ApplicationCommandType Type) x, (string Name, ApplicationCommandType Type) y)
        => x.Type == y.Type && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);

    public int GetHashCode((string Name, ApplicationCommandType Type) obj)
        => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name), obj.Type);
}

/// <summary>
/// A slash registry entry. Holds the built <see cref="ApplicationCommand"/> plus exactly one
/// of <see cref="Flat"/> (single-command handler) or <see cref="Group"/> (group with
/// subcommands). The on-demand flag is not stored here — the parent registry tracks on-demand
/// names in a separate set as the single source of truth.
/// </summary>
internal sealed record SlashEntry(
    string Name,
    ApplicationCommand Command,
    IReadOnlyList<Snowflake> GuildIds,
    CommandInfo? Flat,
    SlashGroupInfo? Group);

/// <summary>
/// A context menu registry entry (user or message). The on-demand flag is not stored here —
/// the parent registry tracks on-demand <c>(name, type)</c> tuples in a separate set.
/// </summary>
internal sealed record ContextMenuEntry(
    string Name,
    ApplicationCommandType Type,
    ApplicationCommand Command,
    ContextMenuCommandInfo Info,
    IReadOnlyList<Snowflake> GuildIds);
