using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Wire-format payload for the full Discord emoji object — as returned by guild emoji listings,
/// <c>GUILD_EMOJIS_UPDATE</c>, and the application/guild emoji CRUD endpoints. Includes the
/// guild-specific fields (<see cref="Roles"/>, <see cref="User"/>, <see cref="RequireColons"/>,
/// <see cref="Managed"/>, <see cref="Available"/>) that the partial <see cref="Emoji"/> doesn't carry.
/// </summary>
/// <remarks>
/// Internal because it's only used inside the runtime; consumers see <see cref="IEmoji"/> via
/// the wrapper layer, or the partial <see cref="Emoji"/> for inputs and component embeds.
/// </remarks>
internal class InternalEmoji
{
    [JsonPropertyName("id")]
    public Snowflake? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("roles")]
    public string[] Roles { get; set; } = [];

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("require_colons")]
    public bool RequireColons { get; set; }

    [JsonPropertyName("managed")]
    public bool Managed { get; set; }

    [JsonPropertyName("animated")]
    public bool Animated { get; set; }

    [JsonPropertyName("available")]
    public bool Available { get; set; }

    public override string ToString()
    {
        if (Id.HasValue)
            return $"{Name}:{Id.Value}";

        return Name ?? string.Empty;
    }
}
