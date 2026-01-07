using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents message activity.
/// </summary>
public class MessageActivity
{
    /// <summary>
    /// Gets or sets the type of message activity.
    /// </summary>
    [JsonPropertyName("type")]
    public MessageActivityType Type { get; set; }

    /// <summary>
    /// Gets or sets the party ID from a Rich Presence event.
    /// </summary>
    [JsonPropertyName("party_id")]
    public string? PartyId { get; set; }
}
