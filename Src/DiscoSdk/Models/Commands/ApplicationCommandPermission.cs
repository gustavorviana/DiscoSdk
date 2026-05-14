using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Single role/user/channel override inside an <see cref="ApplicationCommandPermissions"/>. The
/// <c>permission</c> field on the wire is renamed to <see cref="IApplicationCommandPermission.Allowed"/>
/// on the public surface for clarity (a <c>true</c> means "allowed", not "permission exists").
/// </summary>
internal class ApplicationCommandPermission
{
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; }

    [JsonPropertyName("type")]
    public ApplicationCommandPermissionType Type { get; set; }

    /// <summary>True = allow, false = deny.</summary>
    [JsonPropertyName("permission")]
    public bool Permission { get; set; }
}
