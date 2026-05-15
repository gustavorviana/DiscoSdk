using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Read-only registry of every <see cref="ApplicationCommand"/> discovered by the assembly scan
/// (slash + context menu). Populated once when the registries are built; exposed as a singleton
/// in DI so application code can retrieve a built command body by <c>(name, type)</c> at any
/// point in the bot's lifetime — typically to register an on-demand command on a specific guild
/// via <see cref="DiscoSdk.Rest.Actions.IGuildCommandUpdateFactory"/>.
/// </summary>
/// <remarks>
/// <para>
/// Every discovered command enters the registry regardless of its registration mode (global,
/// guild-specific via <c>GuildIds</c>, or <see cref="OnDemandAttribute">[OnDemand]</see>).
/// Attributes only affect what the <em>auto-registration</em> pipeline does at startup; the
/// registry itself is always complete.
/// </para>
/// <para>
/// Enumeration is type-scoped: callers pass an <see cref="ApplicationCommandType"/> to
/// <see cref="GetAll"/> to delimit exactly which subset they want
/// (chat input vs. user context vs. message context). The on-demand subset is reachable via
/// <see cref="GetOnDemand"/> and <see cref="IsOnDemand"/>.
/// </para>
/// </remarks>
public interface ICommandRegistry
{
    /// <summary>
    /// Returns the built command with the given <paramref name="name"/>, scoped to the
    /// <paramref name="type"/> bucket (chat input / user / message). Names are not unique
    /// across buckets, so the enum delimits exactly which subset is queried. Throws
    /// <see cref="KeyNotFoundException"/> if no such command was discovered.
    /// </summary>
    ApplicationCommand Get(string name, ApplicationCommandType type);

    /// <summary>
    /// Tries to return the built command with the given <paramref name="name"/>, scoped to
    /// the <paramref name="type"/> bucket. Returns <c>true</c> on hit. Like <see cref="Get"/>,
    /// the enum delimits exactly which subset is queried.
    /// </summary>
    bool TryGet(string name, ApplicationCommandType type, out ApplicationCommand command);

    /// <summary>
    /// Returns every discovered command of the requested <paramref name="type"/>:
    /// <see cref="ApplicationCommandType.ChatInput"/> for slash commands,
    /// <see cref="ApplicationCommandType.User"/> for user context menus,
    /// <see cref="ApplicationCommandType.Message"/> for message context menus.
    /// </summary>
    IReadOnlyCollection<ApplicationCommand> GetAll(ApplicationCommandType type);

    /// <summary>
    /// Returns the commands of the requested <paramref name="type"/> that were flagged with
    /// <see cref="OnDemandAttribute">[OnDemand]</see>. The enum delimits exactly which bucket
    /// is enumerated; computed per call from the underlying name sets, so callers that need a
    /// stable snapshot should cache the result themselves.
    /// </summary>
    IReadOnlyCollection<ApplicationCommand> GetOnDemand(ApplicationCommandType type);

    /// <summary>
    /// O(1) membership check for whether <paramref name="name"/> + <paramref name="type"/> was
    /// flagged with <see cref="OnDemandAttribute">[OnDemand]</see>. Returns <c>false</c> for
    /// commands that don't exist (does not throw).
    /// </summary>
    bool IsOnDemand(string name, ApplicationCommandType type);
}
