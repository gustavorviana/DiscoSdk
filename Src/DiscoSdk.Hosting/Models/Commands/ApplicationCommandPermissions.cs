using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Wire-format payload from <c>GET /applications/{app.id}/guilds/{guild.id}/commands/permissions</c>
/// (one entry per command) or
/// <c>GET /applications/{app.id}/guilds/{guild.id}/commands/{cmd.id}/permissions</c> (single).
/// Reference: https://discord.com/developers/docs/interactions/application-commands#application-command-permissions-object
/// </summary>
internal class ApplicationCommandPermissions
{
    /// <summary>Command ID — or the application ID when this row represents the per-guild default for all commands.</summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; }

    [JsonPropertyName("application_id")]
    public Snowflake ApplicationId { get; set; }

    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; set; }

    [JsonPropertyName("permissions")]
    public ApplicationCommandPermission[] Permissions { get; set; } = [];
}
