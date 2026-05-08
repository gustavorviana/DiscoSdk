using DiscoSdk.Commands.Comparisions;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

public sealed class CommandContainer
{
    private static readonly SlashCommandComparer _comparer = new SlashCommandComparer();

    private const int MaxUserContextMenuCommands = 15;
    private const int MaxMessageContextMenuCommands = 15;
    internal HashSet<SlashCommand> GlobalCommands { get; } = new HashSet<SlashCommand>(_comparer);
    internal Dictionary<Snowflake, HashSet<SlashCommand>> GuildCommands { get; } = [];

    public CommandContainer AddGlobal(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        AddGlobal(BuildCommand(configure));
        return this;
    }

    public CommandContainer AddGlobal(params SlashCommand[] commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        AddCommands(GlobalCommands, commands.Where(x => x != null));

        return this;
    }

    public CommandContainer AddGuild(Snowflake guildId, Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        AddGuild(guildId, BuildCommand(configure));
        return this;
    }

    public CommandContainer AddGuild(Snowflake guildId, params SlashCommand[] commands)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(commands);

        if (!GuildCommands.ContainsKey(guildId))
            GuildCommands[guildId] = new HashSet<SlashCommand>(_comparer);

        AddCommands(GuildCommands[guildId], commands.Where(x => x != null));

        return this;
    }

    public CommandContainer AddGlobal(Func<UserCommandBuilder, UserCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        AddGlobal(BuildCommand(configure));
        return this;
    }

    public CommandContainer AddGlobal(Func<MessageCommandBuilder, MessageCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        AddGlobal(BuildCommand(configure));
        return this;
    }

    public CommandContainer AddGuild(Snowflake guildId, Func<UserCommandBuilder, UserCommandBuilder> configure)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        AddGuild(guildId, BuildCommand(configure));
        return this;
    }

    public CommandContainer AddGuild(Snowflake guildId, Func<MessageCommandBuilder, MessageCommandBuilder> configure)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        AddGuild(guildId, BuildCommand(configure));
        return this;
    }

    private static SlashCommand BuildCommand(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new SlashCommandBuilder();
        var configuredBuilder = configure(builder);
        return configuredBuilder.Build();
    }

    private static SlashCommand BuildCommand(Func<UserCommandBuilder, UserCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new UserCommandBuilder();
        var configuredBuilder = configure(builder);
        return configuredBuilder.Build();
    }

    private static SlashCommand BuildCommand(Func<MessageCommandBuilder, MessageCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new MessageCommandBuilder();
        var configuredBuilder = configure(builder);
        return configuredBuilder.Build();
    }

    private static void AddCommands(HashSet<SlashCommand> commands, IEnumerable<SlashCommand> newCommands)
    {
        if (commands is null)
            throw new ArgumentNullException(nameof(commands));

        var pending = newCommands?.Where(c => c != null).ToList()
                     ?? throw new ArgumentNullException(nameof(newCommands));

        if (pending.Count == 0)
            return;

        var existingUser = commands.Count(c => c.Type == ApplicationCommandType.User);
        var existingMessage = commands.Count(c => c.Type == ApplicationCommandType.Message);

        var incomingUser = pending.Count(c => c.Type == ApplicationCommandType.User);
        var incomingMessage = pending.Count(c => c.Type == ApplicationCommandType.Message);

        if (existingUser + incomingUser > MaxUserContextMenuCommands)
            throw new InvalidOperationException(
                $"User context menu command limit of {MaxUserContextMenuCommands} exceeded for this scope.");

        if (existingMessage + incomingMessage > MaxMessageContextMenuCommands)
            throw new InvalidOperationException(
                $"Message context menu command limit of {MaxMessageContextMenuCommands} exceeded for this scope.");

        foreach (var newCommand in pending)
            if (!commands.Add(newCommand))
                throw new InvalidOperationException(
                    $"A command with the same identity is already registered: '{newCommand.Name}'.");
    }
}