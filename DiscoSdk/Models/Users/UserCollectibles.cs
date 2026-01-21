using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Users;

/// <summary>
/// Represents collectibles for a user.
/// </summary>
public class UserCollectibles
{
    /// <summary>
    /// Gets or sets the nameplate.
    /// </summary>
    [JsonPropertyName("nameplate")]
    public Nameplate? Nameplate { get; set; }
}