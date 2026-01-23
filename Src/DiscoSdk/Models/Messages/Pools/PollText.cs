namespace DiscoSdk.Models.Messages.Pools;

public class PollText
{
    /// <summary>
    /// Gets or sets the answer text.
    /// </summary>
    public string? Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional emoji.
    /// </summary>
    public PollEmoji? Emoji { get; set; }
}
