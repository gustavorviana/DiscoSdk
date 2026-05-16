using DiscoSdk.Models.Commands;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.Commands;

/// <summary>
/// Request body for <c>PUT /applications/{app.id}/guilds/{guild.id}/commands/{cmd.id}/permissions</c>.
/// Reference: https://discord.com/developers/docs/interactions/application-commands#edit-application-command-permissions
/// </summary>
internal class EditApplicationCommandPermissionsRequest
{
    /// <summary>Full replacement list of role / user / channel overrides (max 100 per command).</summary>
    [JsonPropertyName("permissions")]
    public ApplicationCommandPermission[] Permissions { get; set; } = [];
}
