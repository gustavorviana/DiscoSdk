using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Pools;

/// <summary>
/// Represents a Discord poll.
/// </summary>
public sealed class Poll
{
    /// <summary>
    /// Gets or sets the poll question.
    /// </summary>
    public PollText Question { get; set; } = new PollText();

    /// <summary>
    /// Gets or sets the available answers.
    /// </summary>
    public List<PollAnswer> Answers { get; set; } = [];

    /// <summary>
    /// Gets or sets the duration in hours.
    /// </summary>
    public int DurationHours { get; set; }

    /// <summary>
    /// Gets or sets whether multiple selections are allowed.
    /// </summary>
    [JsonPropertyName("allow_multiselect")]
    public bool AllowMultiSelect { get; set; }

    /// <summary>
    /// Gets or sets the poll layout.
    /// </summary>
    [JsonPropertyName("layout_type")]
    public PollLayoutType Layout { get; set; } = PollLayoutType.Default;
}