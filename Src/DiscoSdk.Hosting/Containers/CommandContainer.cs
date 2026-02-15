using DiscoSdk.Hosting.Builders;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;

namespace DiscoSdk.Hosting.Containers;

internal class CommandContainer
{
    public List<ApplicationCommand> GlobalCommands { get; } = [];
    public Dictionary<Snowflake, List<ApplicationCommand>> GuildCommands { get; } = [];

    public CommandContainer AddGlobal(params ApplicationCommand[] commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        GlobalCommands.AddRange(commands.Where(x => x != null));

        return this;
    }

    public CommandContainer AddGlobal(Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var command = BuildCommand(configure);
        GlobalCommands.Add(command);
        return this;
    }

    public CommandContainer AddGuild(Snowflake guildId, params ApplicationCommand[] commands)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(commands);

        if (!GuildCommands.ContainsKey(guildId))
            GuildCommands[guildId] = [];

        GuildCommands[guildId].AddRange(commands.Where(x => x != null));

        return this;
    }

    public CommandContainer AddGuild(Snowflake guildId, Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var command = BuildCommand(configure);

        if (!GuildCommands.ContainsKey(guildId))
            GuildCommands[guildId] = [];

        GuildCommands[guildId].Add(command);
        return this;
    }

    private static ApplicationCommand BuildCommand(Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ApplicationCommandBuilder();
        var configuredBuilder = configure(builder);
        return configuredBuilder.Build();
    }
}