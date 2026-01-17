using DiscoSdk.Models;
using DiscoSdk.Models.Builders;
using DiscoSdk.Models.Commands;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Commands;

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
    ICommandUpdateAction AddGlobal(Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure);

    /// <summary>
    /// Adds multiple global application commands to the queue.
    /// </summary>
    /// <param name="commands">The commands to add.</param>
    /// <returns>The current <see cref="CommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGlobal(params ApplicationCommand[] commands);

    /// <summary>
    /// Adds a guild-specific application command to the queue using a fluent builder configuration.
    /// </summary>
    /// <param name="guildId">The guild ID where the command should be registered.</param>
    /// <param name="configure">A function that configures the command builder.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGuild(Snowflake guildId, Func<IApplicationCommandBuilder, IApplicationCommandBuilder> configure);

    /// <summary>
    /// Adds multiple guild-specific application commands to the queue.
    /// </summary>
    /// <param name="guildId">The guild ID where the commands should be registered.</param>
    /// <param name="commands">The commands to add.</param>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction AddGuild(Snowflake guildId, params ApplicationCommand[] commands);

    /// <summary>
    /// Marks that previously registered commands should be deleted before registering new ones.
    /// </summary>
    /// <returns>The current <see cref="ICommandUpdateAction"/> instance for method chaining.</returns>
    ICommandUpdateAction DeletePrevious();
}