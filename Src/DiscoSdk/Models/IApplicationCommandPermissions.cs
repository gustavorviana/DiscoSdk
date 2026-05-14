using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models;

/// <summary>
/// Public read-only view of an application command's per-guild permission overrides.
/// <see cref="Id"/> is the command ID, or the application ID when the row represents the default
/// for all commands in the guild. Reference:
/// https://discord.com/developers/docs/interactions/application-commands#application-command-permissions-object
/// </summary>
public interface IApplicationCommandPermissions
{
    /// <summary>Command ID — or the application ID when this row represents the per-guild default for all commands.</summary>
    Snowflake Id { get; }

    /// <summary>The application the command belongs to.</summary>
    Snowflake ApplicationId { get; }

    /// <summary>The guild the overrides apply to.</summary>
    Snowflake GuildId { get; }

    /// <summary>Individual role / user / channel overrides making up this permission row.</summary>
    IReadOnlyList<IApplicationCommandPermission> Permissions { get; }
}

/// <summary>
/// Single role / user / channel override inside an <see cref="IApplicationCommandPermissions"/>.
/// </summary>
public interface IApplicationCommandPermission
{
    /// <summary>ID of the role / user / channel the override applies to (depending on <see cref="Type"/>).</summary>
    Snowflake Id { get; }

    /// <summary>Whether <see cref="Id"/> is a role, user or channel.</summary>
    ApplicationCommandPermissionType Type { get; }

    /// <summary>True if the command is allowed; false if explicitly denied.</summary>
    bool Allowed { get; }
}
