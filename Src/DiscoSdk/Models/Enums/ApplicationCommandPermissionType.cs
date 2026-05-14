namespace DiscoSdk.Models.Enums;

/// <summary>
/// The kind of entity an <c>ApplicationCommandPermission</c> overrides. Reference:
/// https://discord.com/developers/docs/interactions/application-commands#application-command-permissions-object-application-command-permission-type
/// </summary>
public enum ApplicationCommandPermissionType
{
    /// <summary>Permission applies to a role.</summary>
    Role = 1,

    /// <summary>Permission applies to a single user.</summary>
    User = 2,

    /// <summary>Permission applies to a channel.</summary>
    Channel = 3,
}
