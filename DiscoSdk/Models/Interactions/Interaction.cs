using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Interactions;


/// <summary>
/// Represents a Discord interaction.
/// </summary>
public class Interaction
{
    /// <summary>
    /// Gets or sets the ID of the interaction.
    /// </summary>
    [JsonPropertyName("id")]
    public DiscordId Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the application ID.
    /// </summary>
    [JsonPropertyName("application_id")]
    public DiscordId ApplicationId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of interaction.
    /// </summary>
    [JsonPropertyName("type")]
    public InteractionType Type { get; set; }

    /// <summary>
    /// Gets or sets the interaction data, if the interaction is of type APPLICATION_COMMAND.
    /// </summary>
    [JsonPropertyName("data")]
    public InteractionData? Data { get; set; }

    /// <summary>
    /// Gets or sets the guild ID where the interaction was triggered.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public DiscordId? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the channel ID where the interaction was triggered.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public DiscordId? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the member who triggered the interaction.
    /// </summary>
    [JsonPropertyName("member")]
    public GuildMember? Member { get; set; }

    /// <summary>
    /// Gets or sets the user who triggered the interaction.
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the continuation token for responding to the interaction.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;

    /// <summary>
    /// Gets or sets the read-only property, always 1.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the message this button was attached to.
    /// </summary>
    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    /// <summary>
    /// Gets or sets the selected language of the invoking user.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the guild's preferred locale.
    /// </summary>
    [JsonPropertyName("guild_locale")]
    public string? GuildLocale { get; set; }
}