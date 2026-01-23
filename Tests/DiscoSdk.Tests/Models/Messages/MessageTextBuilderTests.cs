using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Mentions;
using System.Reflection;

namespace DiscoSdk.Tests.Models.Messages;

public class MessageTextBuilderTests
{
	private static HashSet<Mention> GetMentions(MessageTextBuilder builder)
	{
		var type = typeof(MentionBuilderBase<MessageTextBuilder>);
		var property = type.GetProperty("Mentions", BindingFlags.NonPublic | BindingFlags.Instance);
		return (HashSet<Mention>)property!.GetValue(builder)!;
	}

	private static HashSet<Mention> GetMentionsFromMentionBuilder(MentionBuilder builder)
	{
		var type = typeof(MentionBuilderBase<MentionBuilder>);
		var property = type.GetProperty("Mentions", BindingFlags.NonPublic | BindingFlags.Instance);
		return (HashSet<Mention>)property!.GetValue(builder)!;
	}
	[Fact]
	public void Append_WithText_AddsTextToContent()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var text = "Hello, World!";

		// Act
		var result = builder.Append(text);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal(text, builder.ToString());
	}

	[Fact]
	public void Append_WithMultipleCalls_ConcatenatesText()
	{
		// Arrange
		var builder = new MessageTextBuilder();

		// Act
		builder.Append("Hello, ")
			.Append("World!")
			.Append(" How are you?");

		// Assert
		Assert.Equal("Hello, World! How are you?", builder.ToString());
	}

	[Fact]
	public void Append_WithEmptyString_AddsEmptyString()
	{
		// Arrange
		var builder = new MessageTextBuilder();

		// Act
		var result = builder.Append(string.Empty);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal(string.Empty, builder.ToString());
	}

	[Fact]
	public void AppendLine_WithText_AddsTextWithLineBreak()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var text = "First line";

		// Act
		var result = builder.AppendLine(text);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal("First line" + Environment.NewLine, builder.ToString());
	}

	[Fact]
	public void AppendLine_WithMultipleCalls_AddsMultipleLines()
	{
		// Arrange
		var builder = new MessageTextBuilder();

		// Act
		builder.AppendLine("Line 1")
			.AppendLine("Line 2")
			.AppendLine("Line 3");

		// Assert
		var expected = $"Line 1{Environment.NewLine}Line 2{Environment.NewLine}Line 3{Environment.NewLine}";
		Assert.Equal(expected, builder.ToString());
	}

	[Fact]
	public void AppendLine_WithEmptyString_AddsOnlyLineBreak()
	{
		// Arrange
		var builder = new MessageTextBuilder();

		// Act
		var result = builder.AppendLine(string.Empty);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal(Environment.NewLine, builder.ToString());
	}

	[Fact]
	public void AppendMention_WithUserMention_AddsMentionToContentAndTracksIt()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId = new Snowflake(123456789012345678UL);
		var mention = new Mention(MentionType.User, userId, ping: true);

		// Act
		var result = builder.AppendMention(mention);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal($"<@{userId}>", builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Single(mentions);
		Assert.Contains(mention, mentions);
	}

	[Fact]
	public void AppendMention_WithRoleMention_AddsMentionToContentAndTracksIt()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var roleId = new Snowflake(987654321098765432UL);
		var mention = new Mention(MentionType.Role, roleId, ping: true);

		// Act
		var result = builder.AppendMention(mention);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal($"<@&{roleId}>", builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Single(mentions);
		Assert.Contains(mention, mentions);
	}

	[Fact]
	public void AppendMention_WithEveryoneMention_AddsMentionToContentAndTracksIt()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var mention = Mention.Everyone(ping: true);

		// Act
		var result = builder.AppendMention(mention);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal("@everyone", builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Single(mentions);
		Assert.Contains(mention, mentions);
	}

	[Fact]
	public void MentionUser_AddsUserMentionToContent()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId = new Snowflake(123456789012345678UL);

		// Act
		var result = builder.MentionUser(userId);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal($"<@{userId}>", builder.ToString());
		Assert.Single(GetMentions(builder));
	}

	[Fact]
	public void MentionRole_AddsRoleMentionToContent()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var roleId = new Snowflake(987654321098765432UL);

		// Act
		var result = builder.MentionRole(roleId);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal($"<@&{roleId}>", builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Single(mentions);
	}

	[Fact]
	public void MentionEveryone_AddsEveryoneMentionToContent()
	{
		// Arrange
		var builder = new MessageTextBuilder();

		// Act
		var result = builder.MentionEveryone();

		// Assert
		Assert.Same(builder, result);
		Assert.Equal("@everyone", builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Single(mentions);
	}

	[Fact]
	public void ToString_WithTextAndMentions_ReturnsCombinedContent()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId = new Snowflake(123456789012345678UL);
		var roleId = new Snowflake(987654321098765432UL);

		// Act
		builder.Append("Hello ")
			.MentionUser(userId)
			.Append(", you have the ")
			.MentionRole(roleId)
			.Append(" role!");

		// Assert
		var expected = $"Hello <@{userId}>, you have the <@&{roleId}> role!";
		Assert.Equal(expected, builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Equal(2, mentions.Count);
	}

	[Fact]
	public void ToString_WithEmptyBuilder_ReturnsEmptyString()
	{
		// Arrange
		var builder = new MessageTextBuilder();

		// Act
		var result = builder.ToString();

		// Assert
		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void BuildAllowedMentions_WithMentions_ReturnsAllowedMentions()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId = new Snowflake(123456789012345678UL);
		builder.MentionUser(userId)
			.MentionEveryone();

		// Act
		var result = builder.BuildAllowedMentions();

		// Assert
		Assert.NotNull(result);
		Assert.NotNull(result.Parse);
		Assert.Contains("users", result.Parse);
		Assert.Contains("everyone", result.Parse);
		var mentions = GetMentions(builder);
		Assert.Equal(2, mentions.Count);
	}

	[Fact]
	public void BuildAllowedMentions_WithOnlyText_ReturnsNull()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		builder.Append("Hello, World!");

		// Act
		var result = builder.BuildAllowedMentions();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ImplicitConversion_ToMentionBuilder_CopiesMentions()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId = new Snowflake(123456789012345678UL);
		var roleId = new Snowflake(987654321098765432UL);
		builder.MentionUser(userId)
			.MentionRole(roleId)
			.MentionEveryone();

		// Act
		MentionBuilder mentionBuilder = builder;

		// Assert
		var mentionBuilderMentions = GetMentionsFromMentionBuilder(mentionBuilder);
		Assert.Equal(3, mentionBuilderMentions.Count);
		var userIds = mentionBuilderMentions
			.Where(m => m.Type == MentionType.User)
			.Select(m => m.Id)
			.ToList();
		Assert.Contains(userId, userIds);
	}

	[Fact]
	public void FluentInterface_AllowsMethodChaining()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId = new Snowflake(123456789012345678UL);

		// Act
		var result = builder
			.Append("Hello ")
			.MentionUser(userId)
			.AppendLine("!")
			.Append("How are you?");

		// Assert
		Assert.Same(builder, result);
		var expected = $"Hello <@{userId}>!{Environment.NewLine}How are you?";
		Assert.Equal(expected, builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Single(mentions);
	}

	[Fact]
	public void AppendMention_WithSilentMention_AddsToContentButTracksAsSilent()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId = new Snowflake(123456789012345678UL);
		var mention = new Mention(MentionType.User, userId, ping: false);

		// Act
		var result = builder.AppendMention(mention);

		// Assert
		Assert.Same(builder, result);
		Assert.Equal($"<@{userId}>", builder.ToString());
		var mentions = GetMentions(builder);
		Assert.Single(mentions);
		var trackedMention = mentions.First();
		Assert.False(trackedMention.Ping);
	}

	[Fact]
	public void ComplexMessage_WithTextMentionsAndLines_BuildsCorrectly()
	{
		// Arrange
		var builder = new MessageTextBuilder();
		var userId1 = new Snowflake(123456789012345678UL);
		var userId2 = new Snowflake(111111111111111111UL);
		var roleId = new Snowflake(987654321098765432UL);

		// Act
		builder.AppendLine("Welcome to the server!")
			.Append("Hello ")
			.MentionUser(userId1)
			.Append(" and ")
			.MentionUser(userId2, ping: false)
			.AppendLine()
			.Append("You have the ")
			.MentionRole(roleId)
			.Append(" role.")
			.AppendLine()
			.MentionEveryone();

		// Assert
		var content = builder.ToString();
		Assert.Contains("Welcome to the server!", content);
		Assert.Contains($"<@{userId1}>", content);
		Assert.Contains($"<@{userId2}>", content);
		Assert.Contains($"<@&{roleId}>", content);
		Assert.Contains("@everyone", content);

		var allowedMentions = builder.BuildAllowedMentions();
		Assert.NotNull(allowedMentions);
		Assert.NotNull(allowedMentions.Parse);
		Assert.Contains("everyone", allowedMentions.Parse);
		Assert.Contains("roles", allowedMentions.Parse);
		Assert.NotNull(allowedMentions.Users);
		Assert.Contains(userId1.ToString(), allowedMentions.Users);
		Assert.DoesNotContain(userId2.ToString(), allowedMentions.Users);
	}
}

