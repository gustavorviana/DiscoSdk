using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using System.Collections.Frozen;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Mutable companion to <see cref="CommandRegistry"/>. Loader registries
/// (<see cref="SlashCommandRegistry"/>, <see cref="ContextMenuCommandRegistry"/>) receive the
/// builder during construction, call <c>Add*</c> to populate it as they scan assemblies, and
/// the final <see cref="CommandRegistry"/> is produced once by <see cref="Build"/>. Build is
/// idempotent (returns the cached instance on subsequent calls) and locks the builder against
/// further mutations.
/// </summary>
internal sealed class CommandRegistryBuilder
{
    private readonly Dictionary<string, SlashEntry> _slash = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<AutocompleteName, AutocompleteInfo> _autocompletes = [];

    private readonly Dictionary<(string Name, ApplicationCommandType Type), ContextMenuEntry> _contextMenu = new(ContextMenuKeyComparer.Instance);

    private readonly HashSet<string> _onDemandSlash = new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<(string Name, ApplicationCommandType Type)> _onDemandContextMenu = new(ContextMenuKeyComparer.Instance);

    private CommandRegistry? _built;

    public bool HasAutocomplete(AutocompleteName name) => _autocompletes.ContainsKey(name);

    public void AddSlashFlat(
        CommandInfo info,
        ApplicationCommand built,
        IReadOnlyList<Snowflake> guildIds,
        bool isOnDemand)
    {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(built);

        var entry = new SlashEntry(info.Info.Name, built, guildIds, Flat: info, Group: null);
        if (!_slash.TryAdd(entry.Name, entry))
            throw new InvalidOperationException(
                $"Duplicate slash command in registry: '{entry.Name}'. " +
                $"A flat command and a group cannot share the same name.");

        if (isOnDemand)
            _onDemandSlash.Add(entry.Name);
    }

    public void AddSlashGroup(
        SlashGroupInfo group,
        ApplicationCommand built,
        IReadOnlyList<Snowflake> guildIds,
        bool isOnDemand)
    {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(built);

        var entry = new SlashEntry(group.ParentInfo.Name, built, guildIds, Flat: null, Group: group);
        if (!_slash.TryAdd(entry.Name, entry))
            throw new InvalidOperationException(
                $"Duplicate slash command in registry: '{entry.Name}'. " +
                $"A flat command and a group cannot share the same name.");

        if (isOnDemand)
            _onDemandSlash.Add(entry.Name);
    }

    public void AddContextMenu(
        ContextMenuCommandInfo info,
        ApplicationCommand built,
        ApplicationCommandType type,
        IReadOnlyList<Snowflake> guildIds,
        bool isOnDemand)
    {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(built);

        ApplicationCommandTypeGuard.EnsureContextMenu(type, nameof(type));

        var entry = new ContextMenuEntry(info.Name, type, built, info, guildIds);
        var key = (entry.Name, entry.Type);
        if (!_contextMenu.TryAdd(key, entry))
            throw new InvalidOperationException(
                $"Duplicate context menu command in registry: '{entry.Name}' of type {entry.Type}.");

        if (isOnDemand)
            _onDemandContextMenu.Add(key);
    }

    public void AddAutocomplete(AutocompleteName name, AutocompleteInfo info)
    {
        EnsureNotBuilt();
        ArgumentNullException.ThrowIfNull(info);
        if (!_autocompletes.TryAdd(name, info))
            throw new InvalidOperationException(
                $"Duplicate autocomplete handler for command '{info.CommandName}', option '{info.OptionName}'.");
    }

    /// <summary>
    /// Produces the immutable <see cref="CommandRegistry"/>. Idempotent — subsequent calls
    /// return the cached instance. After <c>Build</c> is called, further <c>Add*</c> invocations
    /// throw.
    /// </summary>
    public CommandRegistry Build()
    {
        if (_built is not null)
            return _built;

        _built = new CommandRegistry(
            _slash.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            _autocompletes.ToFrozenDictionary(),
            _contextMenu.ToFrozenDictionary(ContextMenuKeyComparer.Instance),
            _onDemandSlash.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            _onDemandContextMenu.ToFrozenSet(ContextMenuKeyComparer.Instance));

        // Builder cumpriu o papel: o frozen registry agora detém tudo. Libera os buckets dos
        // dicts/sets mutáveis pro GC — o builder vira casca vazia.
        _slash.Clear();
        _autocompletes.Clear();
        _contextMenu.Clear();
        _onDemandSlash.Clear();
        _onDemandContextMenu.Clear();

        return _built;
    }

    private void EnsureNotBuilt()
    {
        if (_built is not null)
            throw new InvalidOperationException(
                "Build has already been called on this CommandRegistryBuilder — further additions are not allowed.");
    }
}
