using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for the <c>PUT</c> half of the application-command-permissions surface. Because
/// the underlying endpoint is the only Discord REST call that <strong>cannot</strong> be made with
/// a bot token, the caller MUST supply a user (or app-credentials) OAuth2 bearer token via
/// <see cref="WithBearerToken"/> before executing — the bearer must hold the
/// <c>applications.commands.permissions.update</c> scope.
/// Reference: https://discord.com/developers/docs/interactions/application-commands#edit-application-command-permissions
/// </summary>
public interface IEditApplicationCommandPermissionsAction : IRestAction<IApplicationCommandPermissions>
{
    /// <summary>
    /// Sets the OAuth2 bearer token used for this single request. Required — Discord rejects
    /// bot-token requests against <c>PUT /commands/{id}/permissions</c> with <c>401</c>.
    /// </summary>
    IEditApplicationCommandPermissionsAction WithBearerToken(string accessToken);

    /// <summary>Adds an allow override for the given role.</summary>
    IEditApplicationCommandPermissionsAction AllowRole(Snowflake roleId);

    /// <summary>Adds a deny override for the given role.</summary>
    IEditApplicationCommandPermissionsAction DenyRole(Snowflake roleId);

    /// <summary>Adds an allow override for the given user.</summary>
    IEditApplicationCommandPermissionsAction AllowUser(Snowflake userId);

    /// <summary>Adds a deny override for the given user.</summary>
    IEditApplicationCommandPermissionsAction DenyUser(Snowflake userId);

    /// <summary>Adds an allow override for the given channel.</summary>
    IEditApplicationCommandPermissionsAction AllowChannel(Snowflake channelId);

    /// <summary>Adds a deny override for the given channel.</summary>
    IEditApplicationCommandPermissionsAction DenyChannel(Snowflake channelId);

    /// <summary>Adds a raw override (escape hatch for less-common cases).</summary>
    IEditApplicationCommandPermissionsAction Add(ApplicationCommandPermissionType type, Snowflake id, bool allowed);

    /// <summary>Removes every accumulated override — the next <c>ExecuteAsync</c> will clear all overrides on the command.</summary>
    IEditApplicationCommandPermissionsAction Clear();
}
