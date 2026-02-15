using DiscoSdk.Commands.Comparisions;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;

namespace DiscoSdk.Commands;

public sealed class CommandContainer
{
    private static readonly  ApplicationCommandComparer _comparer = new ApplicationCommandComparer();
    internal HashSet<ApplicationCommand> GlobalCommands { get; } = new HashSet<ApplicationCommand>(_comparer);
    internal Dictionary<Snowflake, HashSet<ApplicationCommand>> GuildCommands { get; } = [];

    public CommandContainer AddGlobal(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        AddGlobal(BuildCommand(configure));
        return this;
    }

    public CommandContainer AddGlobal(params ApplicationCommand[] commands)
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

    public CommandContainer AddGuild(Snowflake guildId, params ApplicationCommand[] commands)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(commands);

        if (!GuildCommands.ContainsKey(guildId))
            GuildCommands[guildId] = new HashSet<ApplicationCommand>(_comparer);

        AddCommands(GuildCommands[guildId], commands.Where(x => x != null));

        return this;
    }

    private static ApplicationCommand BuildCommand(Func<SlashCommandBuilder, SlashCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new SlashCommandBuilder();
        var configuredBuilder = configure(builder);
        return configuredBuilder.Build();
    }

    private static void AddCommands(HashSet<ApplicationCommand> commands, IEnumerable<ApplicationCommand> newCommands)
    {
        foreach (var newCommand in newCommands)
            if (!commands.Add(newCommand))
                throw new InvalidOperationException(
                    $"A command with the same identity is already registered: '{newCommand.Name}'.");
    }
}