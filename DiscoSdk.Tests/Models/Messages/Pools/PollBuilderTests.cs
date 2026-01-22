using DiscoSdk.Models;
using DiscoSdk.Models.Messages.Pools;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Tests.Models.Messages.Pools;

public class PollBuilderTests
{
	[Fact]
	public void AddAnswer_WithText_AddsAnswerToPoll()
	{
		// Arrange
		var builder = new PollBuilder();
		var answerText = "Opção 1";

		// Act
		var result = builder.AddAnswer(answerText);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Single(poll.Answers);
		Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
		Assert.Null(poll.Answers[0].PoolMedia.Emoji);
	}

	[Fact]
	public void AddAnswer_WithMultipleAnswers_AddsAllAnswers()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		builder.AddAnswer("Opção 1")
			.AddAnswer("Opção 2")
			.AddAnswer("Opção 3");

		// Assert
		var poll = builder.Build();
		Assert.Equal(3, poll.Answers.Count);
		Assert.Equal("Opção 1", poll.Answers[0].PoolMedia.Text);
		Assert.Equal("Opção 2", poll.Answers[1].PoolMedia.Text);
		Assert.Equal("Opção 3", poll.Answers[2].PoolMedia.Text);
	}

	[Fact]
	public void AddAnswer_WithTextAndPollEmoji_AddsAnswerWithEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var answerText = "Opção com emoji";
		var emoji = new PollEmoji
		{
			Id = new Snowflake(123456789012345678UL),
			Name = "smile"
		};

		// Act
		var result = builder.AddAnswer(answerText, emoji);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Single(poll.Answers);
		Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
		Assert.NotNull(poll.Answers[0].PoolMedia.Emoji);
		Assert.Equal(emoji.Id, poll.Answers[0].PoolMedia.Emoji.Id);
		Assert.Equal(emoji.Name, poll.Answers[0].PoolMedia.Emoji.Name);
	}

	[Fact]
	public void AddAnswer_WithTextAndNullPollEmoji_AddsAnswerWithoutEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var answerText = "Opção sem emoji";

		// Act
		var result = builder.AddAnswer(answerText, (PollEmoji?)null);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Single(poll.Answers);
		Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
		Assert.Null(poll.Answers[0].PoolMedia.Emoji);
	}

	[Fact]
	public void AddAnswer_WithTextAndInvalidPollEmoji_AddsAnswerWithoutEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var answerText = "Opção com emoji inválido";
		var invalidEmoji = new PollEmoji
		{
			Id = null,
			Name = null
		};

		// Act
		var result = builder.AddAnswer(answerText, invalidEmoji);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Single(poll.Answers);
		Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
		Assert.Null(poll.Answers[0].PoolMedia.Emoji);
	}

	[Fact]
	public void AddAnswer_WithTextAndPollEmojiWithEmptyIdAndName_AddsAnswerWithoutEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var answerText = "Opção com emoji vazio";
		var emptyEmoji = new PollEmoji
		{
			Id = default(Snowflake),
			Name = string.Empty
		};

		// Act
		var result = builder.AddAnswer(answerText, emptyEmoji);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Single(poll.Answers);
		Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
		Assert.Null(poll.Answers[0].PoolMedia.Emoji);
	}

	[Fact]
	public void AddAnswer_WithTextAndIEmoji_AddsAnswerWithEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var answerText = "Opção com IEmoji";
		var emoji = new MockEmoji
		{
			Id = new Snowflake(987654321098765432UL),
			Name = "custom_emoji"
		};

		// Act
		var result = builder.AddAnswer(answerText, emoji);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Single(poll.Answers);
		Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
		Assert.NotNull(poll.Answers[0].PoolMedia.Emoji);
		Assert.Equal(emoji.Id, poll.Answers[0].PoolMedia.Emoji.Id);
		Assert.Equal(emoji.Name, poll.Answers[0].PoolMedia.Emoji.Name);
	}

	[Fact]
	public void AddAnswer_WithTextAndNullIEmoji_AddsAnswerWithoutEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var answerText = "Opção sem IEmoji";

		// Act
		var result = builder.AddAnswer(answerText, (IEmoji?)null);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Single(poll.Answers);
		Assert.Equal(answerText, poll.Answers[0].PoolMedia.Text);
		Assert.Null(poll.Answers[0].PoolMedia.Emoji);
	}

	[Fact]
	public void SetQuestionText_WithText_SetsQuestionText()
	{
		// Arrange
		var builder = new PollBuilder();
		var questionText = "Qual é a sua opção favorita?";

		// Act
		var result = builder.SetQuestionText(questionText);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Equal(questionText, poll.Question.Text);
	}

	[Fact]
	public void SetQuestionText_WithNull_SetsQuestionTextToNull()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.SetQuestionText(null);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Null(poll.Question.Text);
	}

	[Fact]
	public void SetQuestionText_WithEmptyString_SetsQuestionTextToEmpty()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.SetQuestionText(string.Empty);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Equal(string.Empty, poll.Question.Text);
	}

	[Fact]
	public void SetQuestionEmoji_WithPollEmoji_SetsQuestionEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var emoji = new PollEmoji
		{
			Id = new Snowflake(123456789012345678UL),
			Name = "question"
		};

		// Act
		var result = builder.SetQuestionEmoji(emoji);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.NotNull(poll.Question.Emoji);
		Assert.Equal(emoji.Id, poll.Question.Emoji.Id);
		Assert.Equal(emoji.Name, poll.Question.Emoji.Name);
	}

	[Fact]
	public void SetQuestionEmoji_WithNullPollEmoji_SetsQuestionEmojiToNull()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.SetQuestionEmoji((PollEmoji?)null);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Null(poll.Question.Emoji);
	}

	[Fact]
	public void SetQuestionEmoji_WithInvalidPollEmoji_SetsQuestionEmojiToNull()
	{
		// Arrange
		var builder = new PollBuilder();
		var invalidEmoji = new PollEmoji
		{
			Id = null,
			Name = null
		};

		// Act
		var result = builder.SetQuestionEmoji(invalidEmoji);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Null(poll.Question.Emoji);
	}

	[Fact]
	public void SetQuestionEmoji_WithIEmoji_SetsQuestionEmoji()
	{
		// Arrange
		var builder = new PollBuilder();
		var emoji = new MockEmoji
		{
			Id = new Snowflake(987654321098765432UL),
			Name = "custom_question"
		};

		// Act
		var result = builder.SetQuestionEmoji(emoji);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.NotNull(poll.Question.Emoji);
		Assert.Equal(emoji.Id, poll.Question.Emoji.Id);
		Assert.Equal(emoji.Name, poll.Question.Emoji.Name);
	}

	[Fact]
	public void SetQuestionEmoji_WithNullIEmoji_SetsQuestionEmojiToNull()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.SetQuestionEmoji((IEmoji?)null);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Null(poll.Question.Emoji);
	}

	[Fact]
	public void SetDurationHours_WithHours_SetsDuration()
	{
		// Arrange
		var builder = new PollBuilder();
		var hours = 24;

		// Act
		var result = builder.SetDurationHours(hours);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Equal(hours, poll.DurationHours);
	}

	[Fact]
	public void SetDurationHours_WithZero_SetsDurationToZero()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.SetDurationHours(0);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Equal(0, poll.DurationHours);
	}

	[Fact]
	public void SetDurationHours_WithNegativeValue_SetsDurationToNegative()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.SetDurationHours(-1);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Equal(-1, poll.DurationHours);
	}

	[Fact]
	public void AllowMultiSelect_WithDefault_SetsAllowMultiSelectToTrue()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.AllowMultiSelect();

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.True(poll.AllowMultiSelect);
	}

	[Fact]
	public void AllowMultiSelect_WithTrue_SetsAllowMultiSelectToTrue()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.AllowMultiSelect(true);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.True(poll.AllowMultiSelect);
	}

	[Fact]
	public void AllowMultiSelect_WithFalse_SetsAllowMultiSelectToFalse()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var result = builder.AllowMultiSelect(false);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.False(poll.AllowMultiSelect);
	}

	[Fact]
	public void Build_ReturnsPollInstance()
	{
		// Arrange
		var builder = new PollBuilder();

		// Act
		var poll = builder.Build();

		// Assert
		Assert.NotNull(poll);
		Assert.NotNull(poll.Question);
		Assert.NotNull(poll.Answers);
		Assert.Empty(poll.Answers);
		Assert.Equal(0, poll.DurationHours);
		Assert.False(poll.AllowMultiSelect);
	}

	[Fact]
	public void Build_WithMultipleCalls_ReturnsSameInstance()
	{
		// Arrange
		var builder = new PollBuilder();
		builder.SetQuestionText("Teste")
			.AddAnswer("Opção 1")
			.SetDurationHours(12);

		// Act
		var poll1 = builder.Build();
		var poll2 = builder.Build();

		// Assert
		Assert.Same(poll1, poll2);
		Assert.Equal("Teste", poll1.Question.Text);
		Assert.Single(poll1.Answers);
		Assert.Equal(12, poll1.DurationHours);
	}

	[Fact]
	public void FluentInterface_AllowsMethodChaining()
	{
		// Arrange
		var builder = new PollBuilder();
		var emoji = new PollEmoji
		{
			Id = new Snowflake(123456789012345678UL),
			Name = "smile"
		};

		// Act
		var result = builder
			.SetQuestionText("Qual é a melhor opção?")
			.SetQuestionEmoji(emoji)
			.AddAnswer("Opção 1")
			.AddAnswer("Opção 2", emoji)
			.SetDurationHours(48)
			.AllowMultiSelect(true);

		// Assert
		Assert.Same(builder, result);
		var poll = builder.Build();
		Assert.Equal("Qual é a melhor opção?", poll.Question.Text);
		Assert.NotNull(poll.Question.Emoji);
		Assert.Equal(2, poll.Answers.Count);
		Assert.Equal(48, poll.DurationHours);
		Assert.True(poll.AllowMultiSelect);
	}

	[Fact]
	public void ComplexPoll_WithAllFeatures_BuildsCorrectly()
	{
		// Arrange
		var builder = new PollBuilder();
		var questionEmoji = new PollEmoji
		{
			Id = new Snowflake(111111111111111111UL),
			Name = "question"
		};
		var answerEmoji1 = new PollEmoji
		{
			Id = new Snowflake(222222222222222222UL),
			Name = "option1"
		};
		var answerEmoji2 = new MockEmoji
		{
			Id = new Snowflake(333333333333333333UL),
			Name = "option2"
		};

		// Act
		builder
			.SetQuestionText("Qual é a sua escolha?")
			.SetQuestionEmoji(questionEmoji)
			.AddAnswer("Primeira opção", answerEmoji1)
			.AddAnswer("Segunda opção", answerEmoji2)
			.AddAnswer("Terceira opção")
			.SetDurationHours(72)
			.AllowMultiSelect(false);

		// Assert
		var poll = builder.Build();
		Assert.Equal("Qual é a sua escolha?", poll.Question.Text);
		Assert.NotNull(poll.Question.Emoji);
		Assert.Equal(questionEmoji.Id, poll.Question.Emoji.Id);
		Assert.Equal(questionEmoji.Name, poll.Question.Emoji.Name);
		Assert.Equal(3, poll.Answers.Count);
		Assert.Equal("Primeira opção", poll.Answers[0].PoolMedia.Text);
		Assert.NotNull(poll.Answers[0].PoolMedia.Emoji);
		Assert.Equal(answerEmoji1.Id, poll.Answers[0].PoolMedia.Emoji.Id);
		Assert.Equal("Segunda opção", poll.Answers[1].PoolMedia.Text);
		Assert.NotNull(poll.Answers[1].PoolMedia.Emoji);
		Assert.Equal(answerEmoji2.Id, poll.Answers[1].PoolMedia.Emoji.Id);
		Assert.Equal("Terceira opção", poll.Answers[2].PoolMedia.Text);
		Assert.Null(poll.Answers[2].PoolMedia.Emoji);
		Assert.Equal(72, poll.DurationHours);
		Assert.False(poll.AllowMultiSelect);
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

