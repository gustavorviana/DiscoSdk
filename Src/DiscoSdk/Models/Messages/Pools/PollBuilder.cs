namespace DiscoSdk.Models.Messages.Pools;

public sealed class PollBuilder
{
    private readonly Poll _poll = new();

    public PollBuilder AddAnswer(string text)
    {
        _poll.Answers.Add(new PollAnswer
        {
            PoolMedia = new PollText
            {
                Text = text
            }
        });
        return this;
    }

    public PollBuilder SetQuestionText(string? question)
    {
        _poll.Question.Text = question;
        return this;
    }

    public PollBuilder SetQuestionEmoji(PollEmoji? emoji)
    {
        _poll.Question.Emoji = GetValidEmoji(emoji);
        return this;
    }

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

    public PollBuilder AddAnswer(string text, PollEmoji? emoji)
    {
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

    private static PollEmoji? GetValidEmoji(PollEmoji? emoji)
    {
        if (emoji == null || ((emoji.Id == null || emoji.Id.Value == default) && string.IsNullOrEmpty(emoji.Name)))
            return null;

        return emoji;
    }

    public PollBuilder AddAnswer(string text, IEmoji? emoji)
    {
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

    public PollBuilder SetDurationHours(int hours)
    {
        _poll.DurationHours = hours;
        return this;
    }

    public PollBuilder AllowMultiSelect(bool allow = true)
    {
        _poll.AllowMultiSelect = allow;
        return this;
    }

    public Poll Build()
    {
        return _poll;
    }
}