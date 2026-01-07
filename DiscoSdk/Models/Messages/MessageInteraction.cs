using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a message interaction.
/// </summary>
public class MessageInteraction
{
    /// <summary>
    /// Gets or sets the ID of the interaction.
    /// </summary>
    [JsonPropertyName("id")]
    public DiscordId Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of interaction.
    /// </summary>
    [JsonPropertyName("type")]
    public InteractionType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the application command.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user who invoked the interaction.
    /// </summary>
    [JsonPropertyName("user")]
    public User User { get; set; } = default!;

    /// <summary>
    /// Gets or sets the member who invoked the interaction in the guild.
    /// </summary>
    [JsonPropertyName("member")]
    public GuildMember? Member { get; set; }
}

