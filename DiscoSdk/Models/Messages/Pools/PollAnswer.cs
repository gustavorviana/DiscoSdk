namespace DiscoSdk.Models.Messages.Pools;

// <summary>
/// Represents a single poll answer.
/// </summary>
public sealed class PollAnswer
{
    /// <summary>
    /// Gets or sets the answer text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional emoji.
    /// </summary>
    public PollEmoji? Emoji { get; set; }
}