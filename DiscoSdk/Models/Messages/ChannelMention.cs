using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a channel mention in a message.
/// </summary>
public class ChannelMention
{
    /// <summary>
    /// Gets or sets the ID of the channel.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild containing the channel.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of channel.
    /// </summary>
    [JsonPropertyName("type")]
    public ChannelType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the channel.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}
