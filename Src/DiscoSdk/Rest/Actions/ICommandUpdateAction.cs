using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a fluent builder for queuing and registering Discord application commands.
/// </summary>
public interface ICommandUpdateAction : IRestAction
{
    /// <summary>
    /// Adds a global application command to the queue using a fluent builder configuration.
    /// </summary>
    /// <param name="configure">A function that configures the command builder.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGlobal(Func<SlashCommandBuilder, SlashCommandBuilder> configure);

    /// <summary>
    /// Adds multiple global application commands to the queue.
    /// </summary>
    /// <param name="commands">The commands to add.</param>
    /// <returns>The current <see cref="CommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGlobal(params SlashCommand[] commands);

    /// <summary>
    /// Adds a guild-specific application command to the queue using a fluent builder configuration.
    /// </summary>
    /// <param name="guildId">The guild ID where the command should be registered.</param>
    /// <param name="configure">A function that configures the command builder.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGuild(Snowflake guildId, Func<SlashCommandBuilder, SlashCommandBuilder> configure);

    /// <summary>
    /// Adds multiple guild-specific application commands to the queue.
    /// </summary>
    /// <param name="guildId">The guild ID where the commands should be registered.</param>
    /// <param name="commands">The commands to add.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGuild(Snowflake guildId, params SlashCommand[] commands);

    /// <summary>
    /// Adds a global user context menu command to the queue using a fluent builder configuration.
    /// </summary>
    /// <param name="configure">A function that configures the user command builder.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGlobal(Func<UserCommandBuilder, UserCommandBuilder> configure);

    /// <summary>
    /// Adds a global message context menu command to the queue using a fluent builder configuration.
    /// </summary>
    /// <param name="configure">A function that configures the message command builder.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGlobal(Func<MessageCommandBuilder, MessageCommandBuilder> configure);

    /// <summary>
    /// Adds a guild-specific user context menu command to the queue using a fluent builder configuration.
    /// </summary>
    /// <param name="guildId">The guild ID where the command should be registered.</param>
    /// <param name="configure">A function that configures the user command builder.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGuild(Snowflake guildId, Func<UserCommandBuilder, UserCommandBuilder> configure);

    /// <summary>
    /// Adds a guild-specific message context menu command to the queue using a fluent builder configuration.
    /// </summary>
    /// <param name="guildId">The guild ID where the command should be registered.</param>
    /// <param name="configure">A function that configures the message command builder.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGuild(Snowflake guildId, Func<MessageCommandBuilder, MessageCommandBuilder> configure);

    /// <summary>
    /// Marks that previously registered commands should be deleted before registering new ones.
    /// </summary>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction DeletePrevious();
}