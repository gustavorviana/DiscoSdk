using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Presences;

/// <summary>
/// Represents the client status of a user.
/// </summary>
public class ClientStatus
{
    /// <summary>
    /// Gets or sets the desktop status.
    /// </summary>
    [JsonPropertyName("desktop")]
    public string? Desktop { get; set; }

    /// <summary>
    /// Gets or sets the mobile status.
    /// </summary>
    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    /// <summary>
    /// Gets or sets the web status.
    /// </summary>
    [JsonPropertyName("web")]
    public string? Web { get; set; }
}
