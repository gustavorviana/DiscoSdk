using DiscoSdk.Commands;
using DiscoSdk.Commands.Comparisions;
using DiscoSdk.Hosting.Commands.Localization;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Internal implementation of <see cref="ICommandUpdateScope"/>. Bound to a target (global or
/// a specific guild) at construction; the mode (overwrite vs. append) is mutable via
/// <see cref="SetMode"/> so the session can adjust it on subsequent <c>OpenFor*</c> calls.
/// Implements both the full scope (with <see cref="ApplyAsync"/>) and the add-only builder
/// surface — the builder view is exposed via explicit interface implementation so participants
/// of a session can't see <see cref="ApplyAsync"/>.
/// </summary>
internal sealed class CommandUpdateScope : ICommandUpdateScope
{
    private const int MaxUserContextMenuCommands = 15;
    private const int MaxMessageContextMenuCommands = 15;

    private static readonly SlashCommandComparer _comparer = new();

    private readonly ApplicationCommandClient _client;
    private readonly Snowflake _applicationId;
    private readonly Snowflake? _guildId;
    private bool _overwrite;
    private readonly SlashCommandLocalizer? _slashLocalizer;
    private readonly ContextCommandLocalizer? _contextLocalizer;
    private readonly ICommandRegistry? _registry;
    private readonly ILogger? _logger;

    private readonly HashSet<ApplicationCommand> _commands = new(_comparer);

    public CommandUpdateScope(
        ApplicationCommandClient client,
        Snowflake applicationId,
        Snowflake? guildId,
        bool overwrite,
        SlashCommandLocalizer? slashLocalizer,
        ContextCommandLocalizer? contextLocalizer,
        ICommandRegistry? registry,
        ILogger? logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _applicationId = applicationId;
        _guildId = guildId;
        _overwrite = overwrite;
        _slashLocalizer = slashLocalizer;
        _contextLocalizer = contextLocalizer;
        _registry = registry;
        _logger = logger;
    }

    /// <summary>
    /// Updates the commit mode (overwrite vs. append). Used by <see cref="CommandUpdateSession"/>
    /// to honor last-write-wins when participants re-open the same target with a different mode.
    /// </summary>
    internal void SetMode(bool overwrite) => _overwrite = overwrite;

    public ICommandUpdateScope AddSlash(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        return Add(configure(new SlashCommandBuilder()).Build());
    }

    public ICommandUpdateScope AddContextMenu(ContextMenuType type, Func<ContextMenuBuilder, ContextMenuBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        return Add(configure(new ContextMenuBuilder()).Build(type));
    }

    public ICommandUpdateScope AddFromCatalog(string name, ApplicationCommandType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null, empty, or whitespace.", nameof(name));

        if (_registry is null)
            throw new InvalidOperationException(
                "No command registry is available. Register slash or context menu commands via " +
                "DiscordClientBuilder.WithSlashCommands / WithContextMenuCommands so the registry is populated.");

        if (!_registry.IsOnDemand(name, type))
        {
            if (_registry.TryGet(name, type, out _))
                throw new InvalidOperationException(
                    $"Command '{name}' of type {type} exists in the registry but is not marked [OnDemand]. " +
                    $"AddFromCatalog only resolves on-demand commands. Mark the handler method with [OnDemand] to enable this path.");

            throw new KeyNotFoundException(
                $"No command named '{name}' of type {type} was found in the registry.");
        }

        return Add(_registry.Get(name, type));
    }

    public ICommandUpdateScope Add(ApplicationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var incomingUser = command.Type == ApplicationCommandType.User ? 1 : 0;
        var incomingMessage = command.Type == ApplicationCommandType.Message ? 1 : 0;

        var existingUser = _commands.Count(c => c.Type == ApplicationCommandType.User);
        var existingMessage = _commands.Count(c => c.Type == ApplicationCommandType.Message);

        if (existingUser + incomingUser > MaxUserContextMenuCommands)
            throw new InvalidOperationException(
                $"User context menu command limit of {MaxUserContextMenuCommands} exceeded for this scope.");

        if (existingMessage + incomingMessage > MaxMessageContextMenuCommands)
            throw new InvalidOperationException(
                $"Message context menu command limit of {MaxMessageContextMenuCommands} exceeded for this scope.");

        if (!_commands.Add(command))
            throw new InvalidOperationException(
                $"A command with the same identity is already queued in this scope: '{command.Name}'.");

        return this;
    }

    public ICommandUpdateScope Remove(string name, ApplicationCommandType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null, empty, or whitespace.", nameof(name));

        _commands.RemoveWhere(c =>
            (c.Type ?? ApplicationCommandType.ChatInput) == type &&
            StringComparer.OrdinalIgnoreCase.Equals(c.Name, name));

        return this;
    }

    // ── Builder-only view: delegates to the public scope methods so participants of a session
    //    (which receive ICommandUpdateScopeBuilder) can't see ApplyAsync.
    ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.AddSlash(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
        => AddSlash(configure);
    ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.AddContextMenu(ContextMenuType type, Func<ContextMenuBuilder, ContextMenuBuilder> configure)
        => AddContextMenu(type, configure);
    ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.Add(ApplicationCommand command) => Add(command);
    ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.AddFromCatalog(string name, ApplicationCommandType type) => AddFromCatalog(name, type);
    ICommandUpdateScopeBuilder ICommandUpdateScopeBuilder.Remove(string name, ApplicationCommandType type) => Remove(name, type);

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (_commands.Count == 0 && !_overwrite)
            return;

        ApplyLocalizations();

        if (_overwrite)
        {
            await RegisterAllAsync([.. _commands], cancellationToken);
            return;
        }

        // Append/upsert: read existing → create new, patch changed, no-op equal.
        var existing = await GetAllAsync(cancellationToken);
        var existingByName = existing
            .Where(c => !string.IsNullOrEmpty(c.Name))
            .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

        foreach (var cmd in _commands)
        {
            if (string.IsNullOrEmpty(cmd.Name))
                continue;

            if (!existingByName.TryGetValue(cmd.Name, out var existingCmd))
            {
                await CreateAsync(cmd, cancellationToken);
                continue;
            }

            if (cmd.Equals(existingCmd))
                continue;

            if (existingCmd.Id is null)
            {
                _logger?.LogWarning(
                    "Cannot edit command '{Command}' — Discord returned an existing entry without an ID. Skipping update.",
                    cmd.Name);
                continue;
            }

            await EditAsync(existingCmd.Id.Value, cmd, cancellationToken);
        }
    }

    private Task<List<ApplicationCommand>> RegisterAllAsync(List<ApplicationCommand> commands, CancellationToken ct)
        => _guildId is null
            ? _client.RegisterGlobalCommandsAsync(_applicationId, commands, ct)
            : _client.RegisterGuildCommandsAsync(_applicationId, _guildId.Value, commands, ct);

    private Task<List<ApplicationCommand>> GetAllAsync(CancellationToken ct)
        => _guildId is null
            ? _client.GetGlobalCommandsAsync(_applicationId, ct)
            : _client.GetGuildCommandsAsync(_applicationId, _guildId.Value, ct);

    private Task<ApplicationCommand> CreateAsync(ApplicationCommand command, CancellationToken ct)
        => _guildId is null
            ? _client.CreateGlobalCommandAsync(_applicationId, command, ct)
            : _client.CreateGuildCommandAsync(_applicationId, _guildId.Value, command, ct);

    private Task<ApplicationCommand> EditAsync(Snowflake commandId, ApplicationCommand command, CancellationToken ct)
        => _guildId is null
            ? _client.EditGlobalCommandAsync(_applicationId, commandId, command, ct)
            : _client.EditGuildCommandAsync(_applicationId, _guildId.Value, commandId, command, ct);

    private void ApplyLocalizations()
    {
        if (_slashLocalizer is null && _contextLocalizer is null)
            return;

        foreach (var cmd in _commands)
        {
            if (cmd.Type is ApplicationCommandType.User or ApplicationCommandType.Message)
                _contextLocalizer?.Apply(cmd, _guildId);
            else
                _slashLocalizer?.Apply(cmd, _guildId);
        }
    }
}
