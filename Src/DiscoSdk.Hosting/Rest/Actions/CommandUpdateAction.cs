using DiscoSdk.Commands;
using DiscoSdk.Commands.Localization;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Rest.Actions;
using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Represents a fluent builder for queuing and registering Discord application commands.
/// </summary>
internal class CommandUpdateAction(
    DiscordClient client,
    CommandContainer commandContainer,
    ICommandLocalizationProvider? localizationProvider = null) : RestAction, ICommandUpdateAction
{
    private readonly ApplicationCommandClient _applicationCommandClient = new(client.HttpClient);

    private bool _deletePrevious = false;


    public ICommandUpdateAction AddGlobal(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        commandContainer.AddGlobal(configure);
        return this;
    }

    public ICommandUpdateAction AddGlobal(params SlashCommand[] commands)
    {
        commandContainer.AddGlobal(commands);
        return this;
    }

    public ICommandUpdateAction AddGuild(Snowflake guildId, Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        commandContainer.AddGuild(guildId, configure);
        return this;
    }

    public ICommandUpdateAction AddGuild(Snowflake guildId, params SlashCommand[] commands)
    {
        commandContainer.AddGuild(guildId, commands);
        return this;
    }
    public ICommandUpdateAction AddGlobal(Func<UserCommandBuilder, UserCommandBuilder> configure)
    {
        commandContainer.AddGlobal(configure);
        return this;
    }

    public ICommandUpdateAction AddGlobal(Func<MessageCommandBuilder, MessageCommandBuilder> configure)
    {
        commandContainer.AddGlobal(configure);
        return this;
    }

    public ICommandUpdateAction AddGuild(Snowflake guildId, Func<UserCommandBuilder, UserCommandBuilder> configure)
    {
        commandContainer.AddGuild(guildId, configure);
        return this;
    }

    public ICommandUpdateAction AddGuild(Snowflake guildId, Func<MessageCommandBuilder, MessageCommandBuilder> configure)
    {
        commandContainer.AddGuild(guildId, configure);
        return this;
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
            ApplyLocalizations();
            await RegisterGlobalCommandsAsync(cancellationToken);
            await RegisterGuildCommandsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            client.Logger.Log(LogLevel.Error, ex, "Failed to register commands");
            throw;
        }
    }

    private void ApplyLocalizations()
    {
        if (localizationProvider is null)
            return;

        foreach (var cmd in commandContainer.GlobalCommands)
            CommandLocalizer.Apply(cmd, localizationProvider, guildId: null, logger: client.Logger);

        foreach (var (guildId, commands) in commandContainer.GuildCommands)
            foreach (var cmd in commands)
                CommandLocalizer.Apply(cmd, localizationProvider, guildId, logger: client.Logger);
    }

    /// <summary>
    /// Registers all queued global commands.
    /// </summary>
    private async Task RegisterGlobalCommandsAsync(CancellationToken cancellationToken)
    {
        if (commandContainer.GlobalCommands.Count == 0 && !_deletePrevious)
            return;

        List<SlashCommand> commandsToSend;

        if (!_deletePrevious)
        {
            client.Logger.Log(LogLevel.Debug, "Loading existing global commands from Discord...");
            var existingGlobal = await _applicationCommandClient.GetGlobalCommandsAsync(client.ApplicationId!.Value, cancellationToken);
            commandsToSend = FilterChangedOrNewCommands(commandContainer.GlobalCommands, existingGlobal);

            if (commandsToSend.Count == 0)
            {
                client.Logger.Log(LogLevel.Information, "No changes detected in global commands. Skipping registration.");
                return;
            }
        }
        else
        {
            commandsToSend = [.. commandContainer.GlobalCommands];
        }

        if (commandsToSend.Count == 0 && _deletePrevious)
        {
            client.Logger.Log(LogLevel.Information, "DeletePrevious is true and no global commands are configured. Removing all global commands.");
        }

        client.Logger.Log(LogLevel.Information, "Registering {Count} global command(s) (out of {Total} total)...", commandsToSend.Count, commandContainer.GlobalCommands.Count);
        var registered = await _applicationCommandClient.RegisterGlobalCommandsAsync(client.ApplicationId!.Value, commandsToSend, cancellationToken);
        client.Logger.Log(LogLevel.Information, "Successfully registered {Count} global command(s).", registered.Count);
    }

    /// <summary>
    /// Registers all queued guild-specific commands.
    /// </summary>
    private async Task RegisterGuildCommandsAsync(CancellationToken cancellationToken)
    {
        if (commandContainer.GuildCommands.Count == 0 && !_deletePrevious)
            return;

        foreach (var (guildId, commands) in commandContainer.GuildCommands)
        {
            List<SlashCommand> commandsToSend;

            if (!_deletePrevious)
            {
                client.Logger.Log(LogLevel.Debug, "Loading existing commands for guild {GuildId} from Discord...", guildId);
                var existingGuild = await _applicationCommandClient.GetGuildCommandsAsync(client.ApplicationId!.Value, guildId, cancellationToken);
                commandsToSend = FilterChangedOrNewCommands(commands, existingGuild);

                if (commandsToSend.Count == 0)
                {
                    client.Logger.Log(LogLevel.Information, "No changes detected in commands for guild {GuildId}. Skipping registration.", guildId);
                    continue;
                }
            }
            else
            {
                commandsToSend = [.. commands];

                if (commandsToSend.Count == 0)
                {
                    client.Logger.Log(LogLevel.Information, "DeletePrevious is true and commands for guild {GuildId} is empty. Removing all commands for this guild.", guildId);
                }
            }

            client.Logger.Log(LogLevel.Information, "Registering {Count} command(s) for guild {GuildId} (out of {Total} total)...", commandsToSend.Count, guildId, commands.Count);
            var guildRegistered = await _applicationCommandClient.RegisterGuildCommandsAsync(client.ApplicationId!.Value, guildId, commandsToSend, cancellationToken);
            client.Logger.Log(LogLevel.Information, "Successfully registered {Count} command(s) for guild {GuildId}.", guildRegistered.Count, guildId);
        }
    }

    /// <summary>
    /// Filters the local commands to only include those that are new or have changed compared to existing commands.
    /// Commands that are equivalent are excluded from the result.
    /// </summary>
    private static List<SlashCommand> FilterChangedOrNewCommands(HashSet<SlashCommand> local, List<SlashCommand> existing)
    {
        var existingByName = existing
            .Where(c => !string.IsNullOrEmpty(c.Name))
            .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

        var commandsToSend = new List<SlashCommand>();

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