using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Presences;

/// <summary>
/// Represents user information in a presence.
/// </summary>
public class PresenceUser
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;
}
