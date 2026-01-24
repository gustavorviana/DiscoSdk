using DiscoSdk.Models;
using DiscoSdk.Models.Messages.Pools;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Tests.Models.Messages.Pools;

public class PollBuilderTests
{
    [Fact]
    public void AddAnswer_WithText_AddsAnswerToPoll()
    {
        var builder = new PollBuilder("What is your choice?");
        var answerText = "Option 1";

        var result = builder.SetDurationHours(1).AddAnswer(answerText);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.Single(poll.Answers);
        Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
        Assert.Null(poll.Answers[0].PoolMedia.Emoji);
    }

    [Fact]
    public void AddAnswer_WithMultipleAnswers_AddsAllAnswers()
    {
        var builder = new PollBuilder("What is your choice?");

        builder.SetDurationHours(1)
            .AddAnswer("Option 1")
            .AddAnswer("Option 2")
            .AddAnswer("Option 3");

        var poll = builder.Build();
        Assert.Equal(3, poll.Answers.Count);
        Assert.Equal("Option 1", poll.Answers[0].PoolMedia.Text);
        Assert.Equal("Option 2", poll.Answers[1].PoolMedia.Text);
        Assert.Equal("Option 3", poll.Answers[2].PoolMedia.Text);
    }

    [Fact]
    public void AddAnswer_WithTextAndPollEmoji_AddsAnswerWithEmoji()
    {
        var builder = new PollBuilder("What is your choice?");
        var emoji = new PollEmoji
        {
            Id = new Snowflake(123456789012345678UL),
            Name = "smile"
        };

        var result = builder.SetDurationHours(1).AddAnswer("Option with emoji", emoji);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.Single(poll.Answers);
        Assert.NotNull(poll.Answers[0].PoolMedia.Emoji);
        Assert.Equal(emoji.Id, poll.Answers[0].PoolMedia.Emoji!.Id);
        Assert.Equal(emoji.Name, poll.Answers[0].PoolMedia.Emoji!.Name);
    }

    [Fact]
    public void AddAnswer_WithTextAndNullPollEmoji_AddsAnswerWithoutEmoji()
    {
        var builder = new PollBuilder("What is your choice?");

        var result = builder.SetDurationHours(1).AddAnswer("Option", (PollEmoji?)null);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.Null(poll.Answers[0].PoolMedia.Emoji);
    }

    [Fact]
    public void AddAnswer_WithInvalidPollEmoji_AddsAnswerWithoutEmoji()
    {
        var builder = new PollBuilder("What is your choice?");
        var invalidEmoji = new PollEmoji { Id = null, Name = null };

        var result = builder.SetDurationHours(1).AddAnswer("Option", invalidEmoji);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.Null(poll.Answers[0].PoolMedia.Emoji);
    }

    [Fact]
    public void AddAnswer_WithIEmoji_AddsAnswerWithEmoji()
    {
        var builder = new PollBuilder("What is your choice?");
        var emoji = new MockEmoji
        {
            Id = new Snowflake(987654321098765432UL),
            Name = "custom"
        };

        var result = builder.SetDurationHours(1).AddAnswer("Option", emoji);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.NotNull(poll.Answers[0].PoolMedia.Emoji);
        Assert.Equal(emoji.Id, poll.Answers[0].PoolMedia.Emoji!.Id);
        Assert.Equal(emoji.Name, poll.Answers[0].PoolMedia.Emoji!.Name);
    }

    [Fact]
    public void AddAnswer_WithNullIEmoji_AddsAnswerWithoutEmoji()
    {
        var builder = new PollBuilder("What is your choice?");

        var result = builder.SetDurationHours(1).AddAnswer("Option", (IEmoji?)null);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.Null(poll.Answers[0].PoolMedia.Emoji);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new PollBuilder(null!));
        Assert.Contains("Poll must have a question text", ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyString_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new PollBuilder(string.Empty));
        Assert.Contains("Poll must have a question text", ex.Message);
    }

    [Fact]
    public void SetQuestionEmoji_WithPollEmoji_SetsEmoji()
    {
        var builder = new PollBuilder("Question?");
        builder.SetDurationHours(1).AddAnswer("Answer");

        var emoji = new PollEmoji
        {
            Id = new Snowflake(111111111111111111UL),
            Name = "question"
        };

        var result = builder.SetQuestionEmoji(emoji);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.NotNull(poll.Question.Emoji);
        Assert.Equal(emoji.Id, poll.Question.Emoji!.Id);
    }

    [Fact]
    public void SetQuestionEmoji_WithNull_SetsNull()
    {
        var builder = new PollBuilder("Question?");
        builder.SetDurationHours(1).AddAnswer("Answer");

        var result = builder.SetQuestionEmoji((PollEmoji?)null);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.Null(poll.Question.Emoji);
    }

    [Fact]
    public void SetQuestionEmoji_WithIEmoji_SetsEmoji()
    {
        var builder = new PollBuilder("Question?");
        builder.SetDurationHours(1).AddAnswer("Answer");

        var emoji = new MockEmoji
        {
            Id = new Snowflake(222222222222222222UL),
            Name = "custom"
        };

        var result = builder.SetQuestionEmoji(emoji);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.NotNull(poll.Question.Emoji);
        Assert.Equal(emoji.Id, poll.Question.Emoji!.Id);
    }

    [Fact]
    public void SetDurationHours_WithValidValue_SetsDuration()
    {
        var builder = new PollBuilder("Question?");
        builder.AddAnswer("Answer");

        var result = builder.SetDurationHours(24);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.Equal(24, poll.DurationHours);
    }

    [Fact]
    public void SetDurationHours_WithInvalidLow_Throws()
    {
        var builder = new PollBuilder("Question?");
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetDurationHours(0));
        Assert.Equal("hours", ex.ParamName);
    }

    [Fact]
    public void SetDurationHours_WithInvalidHigh_Throws()
    {
        var builder = new PollBuilder("Question?");
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetDurationHours(169));
        Assert.Equal("hours", ex.ParamName);
    }

    [Fact]
    public void AllowMultiSelect_SetsFlag()
    {
        var builder = new PollBuilder("Question?");
        builder.SetDurationHours(1).AddAnswer("Answer");

        var result = builder.AllowMultiSelect(true);

        Assert.Same(builder, result);

        var poll = builder.Build();
        Assert.True(poll.AllowMultiSelect);
    }

    [Fact]
    public void Build_WithNoAnswers_Throws()
    {
        var builder = new PollBuilder("Question?");
        builder.SetDurationHours(1);

        var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("Poll must have at least one answer", ex.Message);
    }

    [Fact]
    public void Build_WithDurationNotSet_Throws()
    {
        var builder = new PollBuilder("Question?");
        builder.AddAnswer("Answer");

        var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("Poll duration must be between 1 and 168 hours", ex.Message);
    }

    [Fact]
    public void AddAnswer_WithMoreThan10Answers_Throws()
    {
        var builder = new PollBuilder("Question?");
        builder.SetDurationHours(1);

        for (int i = 0; i < 10; i++)
            builder.AddAnswer($"Answer {i}");

        var ex = Assert.Throws<InvalidOperationException>(() => builder.AddAnswer("Overflow"));
        Assert.Contains("Poll cannot have more than 10 answers", ex.Message);
    }

    [Fact]
    public void Build_With10Answers_Works()
    {
        var builder = new PollBuilder("Question?");
        builder.SetDurationHours(1);

        for (int i = 0; i < 10; i++)
            builder.AddAnswer($"Answer {i}");

        var poll = builder.Build();
        Assert.Equal(10, poll.Answers.Count);
    }

    [Fact]
    public void Build_ReturnsSameInstance_OnMultipleCalls()
    {
        var builder = new PollBuilder("Test?");
        builder.SetDurationHours(1).AddAnswer("Answer");

        var poll1 = builder.Build();
        var poll2 = builder.Build();

        Assert.Same(poll1, poll2);
    }

    private class MockEmoji : IEmoji
    {
        public Snowflake Id { get; set; }
        public string? Name { get; set; }
        public DateTimeOffset CreatedAt => DateTimeOffset.UtcNow;
        public string[] Roles => Array.Empty<string>();
        public IUser? User => null;
        public bool RequireColons => false;
        public bool IsManaged => false;
        public bool IsAnimated => false;
        public bool Available => true;
        public IGuild? Guild => null;
        public IEditEmojiAction Edit() => throw new NotImplementedException();
        public IRestAction Delete() => throw new NotImplementedException();
    }
}
