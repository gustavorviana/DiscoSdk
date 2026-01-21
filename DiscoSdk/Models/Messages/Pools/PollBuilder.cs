namespace DiscoSdk.Models.Messages.Pools;

/// <summary>
/// Builder used to create Discord polls.
/// </summary>
public sealed class PollBuilder(string question)
{
    /// <summary>
    /// Internal poll instance being built.
    /// </summary>
    private readonly Poll _poll = new() { Question = question };

    /// <summary>
    /// Adds an answer without emoji.
    /// </summary>
    /// <param name="text">Answer text.</param>
    /// <returns>The current <see cref="PollBuilder"/>.</returns>
    public PollBuilder AddAnswer(string text)
    {
        _poll.Answers.Add(new PollAnswer
        {
            Text = text
        });

        return this;
    }

    /// <summary>
    /// Adds an answer with a unicode emoji.
    /// </summary>
    /// <param name="text">Answer text.</param>
    /// <param name="emojiName">Emoji name or unicode value.</param>
    /// <returns>The current <see cref="PollBuilder"/>.</returns>
    public PollBuilder AddAnswer(string text, string emojiName)
    {
        _poll.Answers.Add(new PollAnswer
        {
            Text = text,
            Emoji = new PollEmoji
            {
                Name = emojiName
            }
        });

        return this;
    }

    /// <summary>
    /// Adds an answer with a custom emoji.
    /// </summary>
    /// <param name="text">Answer text.</param>
    /// <param name="emojiId">Custom emoji id.</param>
    /// <param name="emojiName">Optional emoji name.</param>
    /// <returns>The current <see cref="PollBuilder"/>.</returns>
    public PollBuilder AddAnswer(string text, ulong emojiId, string? emojiName = null)
    {
        _poll.Answers.Add(new PollAnswer
        {
            Text = text,
            Emoji = new PollEmoji
            {
                Id = emojiId,
                Name = emojiName
            }
        });

        return this;
    }

    /// <summary>
    /// Sets the poll duration in hours.
    /// </summary>
    /// <param name="hours">Duration in hours.</param>
    /// <returns>The current <see cref="PollBuilder"/>.</returns>
    public PollBuilder SetDurationHours(int hours)
    {
        _poll.DurationHours = hours;
        return this;
    }

    /// <summary>
    /// Defines whether multiple answers can be selected.
    /// </summary>
    /// <param name="allow">True to allow multiple selections.</param>
    /// <returns>The current <see cref="PollBuilder"/>.</returns>
    public PollBuilder AllowMultiSelect(bool allow = true)
    {
        _poll.AllowMultiSelect = allow;
        return this;
    }

    /// <summary>
    /// Builds the poll instance.
    /// </summary>
    /// <returns>The built <see cref="Poll"/>.</returns>
    public Poll Build()
    {
        return _poll;
    }
}