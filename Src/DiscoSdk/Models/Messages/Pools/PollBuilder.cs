namespace DiscoSdk.Models.Messages.Pools;

/// <summary>
/// Fluent builder for creating Discord polls.
/// Provides methods to configure poll properties such as question, answers, duration, and multi-select options.
/// </summary>
public sealed class PollBuilder
{
    private readonly Poll _poll = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PollBuilder"/> class with the specified question text.
    /// </summary>
    /// <param name="question">The question text. Must be between 1 and 300 characters if provided. Can be null or empty, but will be validated in <see cref="Build"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when question exceeds 300 characters.</exception>
    public PollBuilder(string question)
    {
        if (string.IsNullOrEmpty(question))
            throw new InvalidOperationException("Poll must have a question text.");

        if (question.Length > 300)
            throw new ArgumentOutOfRangeException(nameof(question), $"Question text cannot exceed 300 characters. Current length: {question.Length}.");

        _poll.Question.Text = question;
    }

    /// <summary>
    /// Adds an answer option to the poll.
    /// </summary>
    /// <param name="text">The answer text. Must be between 1 and 55 characters.</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when text is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when text exceeds 55 characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add more than 10 answers.</exception>
    public PollBuilder AddAnswer(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Answer text cannot be null, empty, or contain only whitespace.", nameof(text));
        if (text.Length > 55)
            throw new ArgumentOutOfRangeException(nameof(text), $"Answer text cannot exceed 55 characters. Current length: {text.Length}.");

        if (_poll.Answers.Count >= 10)
            throw new InvalidOperationException("Poll cannot have more than 10 answers.");

        _poll.Answers.Add(new PollAnswer
        {
            PoolMedia = new PollText
            {
                Text = text
            }
        });
        return this;
    }

    /// <summary>
    /// Sets the emoji for the poll question.
    /// </summary>
    /// <param name="emoji">The emoji to set. Can be null to remove the emoji.</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    public PollBuilder SetQuestionEmoji(PollEmoji? emoji)
    {
        _poll.Question.Emoji = GetValidEmoji(emoji);
        return this;
    }

    /// <summary>
    /// Sets the emoji for the poll question from an <see cref="IEmoji"/> instance.
    /// </summary>
    /// <param name="emoji">The emoji to set. Can be null to remove the emoji.</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    public PollBuilder SetQuestionEmoji(IEmoji? emoji)
    {
        if (emoji == null)
        {
            _poll.Question.Emoji = null;
            return this;
        }

        _poll.Question.Emoji = new PollEmoji
        {
            Id = emoji.Id,
            Name = emoji.Name
        };
        return this;
    }

    /// <summary>
    /// Adds an answer option to the poll with an emoji.
    /// </summary>
    /// <param name="text">The answer text. Must be between 1 and 55 characters.</param>
    /// <param name="emoji">The emoji to display with the answer. Can be null.</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when text is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when text exceeds 55 characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add more than 10 answers.</exception>
    public PollBuilder AddAnswer(string text, PollEmoji? emoji)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Answer text cannot be null, empty, or contain only whitespace.", nameof(text));
        if (text.Length > 55)
            throw new ArgumentOutOfRangeException(nameof(text), $"Answer text cannot exceed 55 characters. Current length: {text.Length}.");

        if (_poll.Answers.Count >= 10)
            throw new InvalidOperationException("Poll cannot have more than 10 answers.");

        _poll.Answers.Add(new PollAnswer
        {
            PoolMedia = new PollText
            {
                Text = text,
                Emoji = GetValidEmoji(emoji)
            }
        });

        return this;
    }

    /// <summary>
    /// Validates and normalizes a <see cref="PollEmoji"/> instance.
    /// Returns null if the emoji is invalid (both Id and Name are null/empty/default).
    /// </summary>
    /// <param name="emoji">The emoji to validate.</param>
    /// <returns>A valid <see cref="PollEmoji"/> instance, or null if the emoji is invalid.</returns>
    private static PollEmoji? GetValidEmoji(PollEmoji? emoji)
    {
        if (emoji == null || ((emoji.Id == null || emoji.Id.Value == default) && string.IsNullOrEmpty(emoji.Name)))
            return null;

        return emoji;
    }

    /// <summary>
    /// Adds an answer option to the poll with an emoji from an <see cref="IEmoji"/> instance.
    /// </summary>
    /// <param name="text">The answer text. Must be between 1 and 55 characters.</param>
    /// <param name="emoji">The emoji to display with the answer. Can be null.</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when text is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when text exceeds 55 characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add more than 10 answers.</exception>
    public PollBuilder AddAnswer(string text, IEmoji? emoji)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Answer text cannot be null, empty, or contain only whitespace.", nameof(text));
        if (text.Length > 55)
            throw new ArgumentOutOfRangeException(nameof(text), $"Answer text cannot exceed 55 characters. Current length: {text.Length}.");

        if (_poll.Answers.Count >= 10)
            throw new InvalidOperationException("Poll cannot have more than 10 answers.");

        _poll.Answers.Add(new PollAnswer
        {
            PoolMedia = new PollText
            {
                Text = text,
                Emoji = emoji == null ? null : new PollEmoji
                {
                    Id = emoji.Id,
                    Name = emoji.Name
                }
            }
        });
        return this;
    }

    /// <summary>
    /// Sets the duration of the poll in hours.
    /// </summary>
    /// <param name="hours">The duration in hours. Must be between 1 and 168 (7 days).</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when hours is less than 1 or greater than 168.</exception>
    public PollBuilder SetDurationHours(int hours)
    {
        if (hours < 1)
            throw new ArgumentOutOfRangeException(nameof(hours), "Duration must be at least 1 hour.");
        if (hours > 168)
            throw new ArgumentOutOfRangeException(nameof(hours), "Duration cannot exceed 168 hours (7 days).");

        _poll.DurationHours = hours;
        return this;
    }

    /// <summary>
    /// Sets whether multiple selections are allowed in the poll.
    /// </summary>
    /// <param name="allow">True to allow multiple selections, false to allow only single selection. Defaults to true.</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    public PollBuilder AllowMultiSelect(bool allow = true)
    {
        _poll.AllowMultiSelect = allow;
        return this;
    }

    /// <summary>
    /// Sets the layout type for the poll.
    /// </summary>
    /// <param name="layout">The layout type to use. Defaults to <see cref="PollLayoutType.Default"/>.</param>
    /// <returns>The current <see cref="PollBuilder"/> instance for method chaining.</returns>
    public PollBuilder SetLayout(PollLayoutType layout)
    {
        _poll.Layout = layout;
        return this;
    }

    /// <summary>
    /// Builds and validates the poll according to Discord API constraints.
    /// </summary>
    /// <returns>The built <see cref="Poll"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the poll has no answers, or has invalid configuration.</exception>
    public Poll Build()
    {
        if (_poll.Answers.Count == 0)
            throw new InvalidOperationException("Poll must have at least one answer.");

        if (_poll.Answers.Count > 10)
            throw new InvalidOperationException($"Poll cannot have more than 10 answers. Current count: {_poll.Answers.Count}.");

        foreach (var answer in _poll.Answers)
        {
            if (string.IsNullOrWhiteSpace(answer.PoolMedia.Text))
                throw new InvalidOperationException("All poll answers must have non-empty text.");

            if (answer.PoolMedia.Text!.Length > 55)
                throw new InvalidOperationException($"Poll answer text cannot exceed 55 characters. Current length: {answer.PoolMedia.Text.Length}.");
        }

        if (_poll.DurationHours < 1 || _poll.DurationHours > 168)
            throw new InvalidOperationException($"Poll duration must be between 1 and 168 hours. Current value: {_poll.DurationHours}.");

        return _poll;
    }
}