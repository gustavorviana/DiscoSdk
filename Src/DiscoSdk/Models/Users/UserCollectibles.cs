using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Users;

/// <summary>Read-only collectibles bag attached to a user (currently just the nameplate).</summary>
public class UserCollectibles
{
    /// <summary>Nameplate the user has equipped, if any.</summary>
    [JsonPropertyName("nameplate")]
    public Nameplate? Nameplate { get; init; }
}
