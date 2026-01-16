using DiscoSdk.Commands;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Logging;
using DiscoSdk.Models.Builders;
using DiscoSdk.Models.Commands;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Represents a fluent builder for queuing and registering Discord application commands.
/// </summary>
internal class CommandUpdateAction(DiscordClient client) : RestAction, ICommandUpdateAction
{
    private readonly ApplicationCommandClient _applicationCommandClient = new(client.HttpClient);

    private readonly List<ApplicationCommand> _globalCommands = [];
    private readonly Dictionary<string, List<ApplicationCommand>> _guildCommands = [];
    private bool _deletePrevious = false;


    public ICommandUpdateAction AddGlobal(params ApplicationCommand[] commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        _globalCommands.AddRange(commands.Where(x => x != null));

        return this;
    }

    public ICommandUpdateAction AddGlobal(Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var command = BuildCommand(configure);
        _globalCommands.Add(command);
        return this;
    }

    public ICommandUpdateAction AddGuild(string guildId, params ApplicationCommand[] commands)
    {
        if (string.IsNullOrEmpty(guildId))
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(commands);

        if (!_guildCommands.ContainsKey(guildId))
            _guildCommands[guildId] = [];

        _guildCommands[guildId].AddRange(commands.Where(x => x != null));

        return this;
    }

    public ICommandUpdateAction AddGuild(string guildId, Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure)
    {
        if (string.IsNullOrEmpty(guildId))
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var command = BuildCommand(configure);

        if (!_guildCommands.ContainsKey(guildId))
            _guildCommands[guildId] = [];

        _guildCommands[guildId].Add(command);
        return this;
    }

    private static ApplicationCommand BuildCommand(Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ApplicationCommandBuilder();
        var configuredBuilder = configure(builder);
        return configuredBuilder.Build();
    }
    public ICommandUpdateAction DeletePrevious()
    {
        _deletePrevious = true;
        return this;
    }


    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await RegisterGlobalCommandsAsync(cancellationToken);
            await RegisterGuildCommandsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            client.Logger.Log(LogLevel.Error, "Failed to register commands", ex);
            throw;
        }
    }

    /// <summary>
    /// Registers all queued global commands.
    /// </summary>
    private async Task RegisterGlobalCommandsAsync(CancellationToken cancellationToken)
    {
        if (_globalCommands.Count == 0 && !_deletePrevious)
            return;

        List<ApplicationCommand> commandsToSend;

        if (!_deletePrevious)
        {
            client.Logger.Log(LogLevel.Debug, "Loading existing global commands from Discord...");
            var existingGlobal = await _applicationCommandClient.GetGlobalCommandsAsync(client.ApplicationId!, cancellationToken);
            commandsToSend = FilterChangedOrNewCommands(_globalCommands, existingGlobal);

            if (commandsToSend.Count == 0)
            {
                client.Logger.Log(LogLevel.Information, "No changes detected in global commands. Skipping registration.");
                return;
            }
        }
        else
        {
            commandsToSend = _globalCommands;
        }

        if (commandsToSend.Count == 0 && _deletePrevious)
        {
            client.Logger.Log(LogLevel.Information, "DeletePrevious is true and no global commands are configured. Removing all global commands.");
        }

        client.Logger.Log(LogLevel.Information, $"Registering {commandsToSend.Count} global command(s) (out of {_globalCommands.Count} total)...");
        var registered = await _applicationCommandClient.RegisterGlobalCommandsAsync(client.ApplicationId!, commandsToSend, cancellationToken);
        client.Logger.Log(LogLevel.Information, $"Successfully registered {registered.Count} global command(s).");
    }

    /// <summary>
    /// Registers all queued guild-specific commands.
    /// </summary>
    private async Task RegisterGuildCommandsAsync(CancellationToken cancellationToken)
    {
        if (_guildCommands.Count == 0 && !_deletePrevious)
            return;

        foreach (var (guildId, commands) in _guildCommands)
        {
            List<ApplicationCommand> commandsToSend;

            if (!_deletePrevious)
            {
                client.Logger.Log(LogLevel.Debug, $"Loading existing commands for guild {guildId} from Discord...");
                var existingGuild = await _applicationCommandClient.GetGuildCommandsAsync(client.ApplicationId!, guildId, cancellationToken);
                commandsToSend = FilterChangedOrNewCommands(commands, existingGuild);

                if (commandsToSend.Count == 0)
                {
                    client.Logger.Log(LogLevel.Information, $"No changes detected in commands for guild {guildId}. Skipping registration.");
                    continue;
                }
            }
            else
            {
                commandsToSend = commands;

                if (commandsToSend.Count == 0)
                {
                    client.Logger.Log(LogLevel.Information, $"DeletePrevious is true and commands for guild {guildId} is empty. Removing all commands for this guild.");
                }
            }

            client.Logger.Log(LogLevel.Information, $"Registering {commandsToSend.Count} command(s) for guild {guildId} (out of {commands.Count} total)...");
            var guildRegistered = await _applicationCommandClient.RegisterGuildCommandsAsync(client.ApplicationId!, guildId, commandsToSend, cancellationToken);
            client.Logger.Log(LogLevel.Information, $"Successfully registered {guildRegistered.Count} command(s) for guild {guildId}.");
        }
    }

    /// <summary>
    /// Filters the local commands to only include those that are new or have changed compared to existing commands.
    /// Commands that are equivalent are excluded from the result.
    /// </summary>
    private static List<ApplicationCommand> FilterChangedOrNewCommands(List<ApplicationCommand> local, List<ApplicationCommand> existing)
    {
        var existingByName = existing
            .Where(c => !string.IsNullOrEmpty(c.Name))
            .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

        var commandsToSend = new List<ApplicationCommand>();

        foreach (var localCmd in local)
        {
            if (string.IsNullOrEmpty(localCmd.Name) || !existingByName.TryGetValue(localCmd.Name, out var existingCmd))
            {
                commandsToSend.Add(localCmd);
                continue;
            }

            // Command exists, check if it changed
            if (!localCmd.Equals(existingCmd))
            {
                // Command changed, reuse ID and send update
                localCmd.Id = existingCmd.Id;
                commandsToSend.Add(localCmd);
            }
        }

        if (commandsToSend.Count == 0)
            return commandsToSend;

        foreach (var remaining in existingByName.Values)
        {
            if (!local.Any(c => string.Equals(c.Name, remaining.Name, StringComparison.OrdinalIgnoreCase)))
            {
                commandsToSend.Add(remaining);
            }
        }

        return commandsToSend;
    }
}