namespace DiscoSdk.Models.Messages.Pools;

/// <summary>
/// Represents an emoji attached to a poll answer.
/// </summary>
public sealed class PollEmoji
{
    /// <summary>
    /// Gets or sets the emoji id (custom emoji).
    /// </summary>
    public Snowflake? Id { get; set; }

    /// <summary>
    /// Gets or sets the emoji name.
    /// </summary>
    public string? Name { get; set; }
}