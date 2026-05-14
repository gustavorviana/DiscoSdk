using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Public surface for application-command operations scoped to a guild. Exposed via
/// <see cref="IGuild.Commands"/>. Only per-guild command-permission endpoints today; future
/// expansion (registration, single-command CRUD) is the natural place to grow.
/// </summary>
public interface IGuildCommands
{
    /// <summary>
    /// Lists permission overrides for every application command in this guild. Bot-token auth.
    /// </summary>
    IRestAction<IReadOnlyList<IApplicationCommandPermissions>> GetAllPermissions();

    /// <summary>
    /// Gets the permission overrides for a single application command. Bot-token auth.
    /// </summary>
    IRestAction<IApplicationCommandPermissions> GetPermissions(Snowflake commandId);

    /// <summary>
    /// Builds a request to replace the permission overrides for a single application command.
    /// The endpoint requires a user OAuth2 bearer token with the
    /// <c>applications.commands.permissions.update</c> scope — call
    /// <see cref="IEditApplicationCommandPermissionsAction.WithBearerToken"/> before executing.
    /// </summary>
    IEditApplicationCommandPermissionsAction EditPermissions(Snowflake commandId);
}
