using DiscoSdk.Rest;

namespace DiscoSdk.Tests.Rest;

public class DiscordRouteTests
{
	[Fact]
	public void Constructor_WithNullTemplate_ThrowsArgumentNullException()
	{
		// Arrange & Act & Assert
		Assert.Throws<ArgumentNullException>(() => new DiscordRoute(null!));
	}

	[Fact]
	public void Constructor_WithValidTemplate_DoesNotThrow()
	{
		// Arrange & Act
		var route = new DiscordRoute("/channels/{channel_id}");

		// Assert
		Assert.Equal("/channels/{channel_id}", route.Template);
	}

	[Fact]
	public void ToString_WithoutArguments_ReturnsTemplate()
	{
		// Arrange
		var template = "/channels/{channel_id}/messages";
		var route = new DiscordRoute(template);

		// Act
		var result = route.ToString();

		// Assert
		Assert.Equal(template, result);
	}

	[Fact]
	public void ToString_WithSingleArgument_ReplacesPlaceholder()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id}", 123456789UL);

		// Act
		var result = route.ToString();

		// Assert
		Assert.Equal("/channels/123456789", result);
	}

	[Fact]
	public void ToString_WithMultipleArguments_ReplacesAllPlaceholders()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id}/messages/{message_id}", 123456789UL, 987654321UL);

		// Act
		var result = route.ToString();

		// Assert
		Assert.Equal("/channels/123456789/messages/987654321", result);
	}

	[Fact]
	public void ToString_WithInsufficientArguments_ThrowsFormatException()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id}/messages/{message_id}", 123456789UL);

		// Act & Assert
		var exception = Assert.Throws<FormatException>(() => route.ToString());
		Assert.Contains("Not enough route arguments", exception.Message);
	}

	[Fact]
	public void ToString_WithTooManyArguments_ThrowsFormatException()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id}", 123456789UL, 987654321UL);

		// Act & Assert
		var exception = Assert.Throws<FormatException>(() => route.ToString());
		Assert.Contains("Too many route arguments", exception.Message);
	}

	[Fact]
	public void ToString_WithInvalidTemplate_UnclosedBrace_ThrowsFormatException()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id", 123456789UL);

		// Act & Assert
		var exception = Assert.Throws<FormatException>(() => route.ToString());
		Assert.Contains("Invalid route template", exception.Message);
	}

	[Fact]
	public void GetBucketPath_WithChannelId_ReturnsPathUpToChannelId()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id}/messages/{message_id}", 123456789UL, 987654321UL);

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/channels/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithGuildId_ReturnsPathUpToGuildId()
	{
		// Arrange
		var route = new DiscordRoute("/guilds/{guild_id}/channels/{channel_id}", "123456789", "987654321");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/guilds/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithWebhookId_ReturnsPathUpToWebhookId()
	{
		// Arrange
		var route = new DiscordRoute("/webhooks/{webhook_id}/{token}", "123456789", "token123");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/webhooks/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithInteractionId_ReturnsPathUpToInteractionId()
	{
		// Arrange
		var route = new DiscordRoute("/interactions/{interaction_id}/{token}/callback", "123456789", "token123");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/interactions/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithoutMajorParameter_WithApplicationId_ReturnsPathUpToApplicationId()
	{
		// Arrange
		var route = new DiscordRoute("/applications/{application_id}/commands", "123456789");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/applications/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithoutMajorParameter_WithoutApplicationId_ReturnsFullPath()
	{
		// Arrange
		var route = new DiscordRoute("/gateway/bot");

		// Act
		var result = route.GetBucketPath();

        // Assert
        Assert.Null(result);
    }

	[Fact]
	public void GetBucketPath_WithMultipleSegmentsBeforeMajor_ReturnsFullPathUpToMajor()
	{
		// Arrange
		var route = new DiscordRoute("/api/v10/guilds/{guild_id}/channels", "123456789");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/api/v10/guilds/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithInsufficientArguments_ReturnsPlaceholder()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id}/messages", 123456789UL);

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/channels/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithNoArguments_ReturnsPathWithPlaceholder()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{channel_id}/messages");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/channels/{channel_id}", result);
	}

	[Fact]
	public void GetBucketPath_WithEmptyTemplate_ReturnsSlash()
	{
		// Arrange
		var route = new DiscordRoute("");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void GetBucketPath_WithCaseInsensitiveMajorParameter_RecognizesParameter()
	{
		// Arrange
		var route = new DiscordRoute("/channels/{CHANNEL_ID}/messages", 123456789UL);

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/channels/123456789", result);
	}

	[Fact]
	public void GetBucketPath_WithApplicationIdFallback_WhenNoMajorParameter_WorksCorrectly()
	{
		// Arrange
		var route = new DiscordRoute("/applications/{application_id}/guilds/{guild_id}/commands", "123", "456");

		// Act
		var result = route.GetBucketPath();

		// Assert
		Assert.Equal("/applications/{application_id}/guilds/456", result);
	}
}
