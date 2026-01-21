namespace DiscoSdk.Models.Messages.Pools;

/// <summary>
/// Represents a Discord poll.
/// </summary>
public sealed class Poll
{
    /// <summary>
    /// Gets or sets the poll question.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the available answers.
    /// </summary>
    public List<PollAnswer> Answers { get; set; } = new();

    /// <summary>
    /// Gets or sets the duration in hours.
    /// </summary>
    public int DurationHours { get; set; }

    /// <summary>
    /// Gets or sets whether multiple selections are allowed.
    /// </summary>
    public bool AllowMultiSelect { get; set; }

    /// <summary>
    /// Gets or sets the poll layout.
    /// </summary>
    public PollLayoutType Layout { get; set; } = PollLayoutType.Default;
}