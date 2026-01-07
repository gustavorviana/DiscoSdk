using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a reaction to a message.
/// </summary>
public class Reaction
{
    /// <summary>
    /// Gets or sets the number of times this emoji has been used to react.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user reacted using this emoji.
    /// </summary>
    [JsonPropertyName("me")]
    public bool Me { get; set; }

    /// <summary>
    /// Gets or sets the emoji information.
    /// </summary>
    [JsonPropertyName("emoji")]
    public Emoji Emoji { get; set; } = default!;
}
