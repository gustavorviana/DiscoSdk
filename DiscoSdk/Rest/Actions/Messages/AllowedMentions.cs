using System.Text.Json.Serialization;

namespace DiscoSdk.Rest.Actions.Messages;

/// <summary>
/// Represents allowed mentions configuration for a message.
/// </summary>
public class AllowedMentions
{
    /// <summary>
    /// Gets or sets an array of allowed mention types to parse from the content.
    /// Valid values: "roles", "users", "everyone"
    /// </summary>
    [JsonPropertyName("parse")]
    public string[]? Parse { get; set; }

    /// <summary>
    /// Gets or sets an array of role IDs to mention (max 100).
    /// </summary>
    [JsonPropertyName("roles")]
    public string[]? Roles { get; set; }

    /// <summary>
    /// Gets or sets an array of user IDs to mention (max 100).
    /// </summary>
    [JsonPropertyName("users")]
    public string[]? Users { get; set; }

    /// <summary>
    /// Gets or sets whether to mention the user who replied.
    /// </summary>
    [JsonPropertyName("replied_user")]
    public bool? RepliedUser { get; set; }
}