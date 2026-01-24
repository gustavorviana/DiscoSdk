using DiscoSdk.Models;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Tests.Models.Messages;

public class EmbedBuilderTests
{
	[Fact]
	public void Constructor_WithValidTitle_CreatesBuilder()
	{
		// Arrange
		var title = "Test Title";

		// Act
		var builder = new EmbedBuilder(title);

		// Assert
		Assert.NotNull(builder);
		var embed = builder.Build();
		Assert.Equal(title, embed.Title);
	}

	[Fact]
	public void Constructor_WithTitleAndUrl_CreatesBuilderWithUrl()
	{
		// Arrange
		var title = "Test Title";
		var url = "https://example.com";

		// Act
		var builder = new EmbedBuilder(title, url);

		// Assert
		Assert.NotNull(builder);
		var embed = builder.Build();
		Assert.Equal(title, embed.Title);
		Assert.Equal(url, embed.Url);
	}

	[Fact]
	public void Constructor_WithNullTitle_ThrowsArgumentException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new EmbedBuilder(null!));
		Assert.Contains("Title cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("title", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithEmptyTitle_ThrowsArgumentException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new EmbedBuilder(string.Empty));
		Assert.Contains("Title cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("title", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithWhitespaceTitle_ThrowsArgumentException()
	{
		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new EmbedBuilder("   "));
		Assert.Contains("Title cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("title", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithTitleExceeding256Characters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var title = new string('a', 257);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new EmbedBuilder(title));
		Assert.Contains("Title cannot exceed 256 characters", exception.Message);
		Assert.Equal("title", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithInvalidUrl_ThrowsArgumentException()
	{
		// Arrange
		var title = "Test Title";
		var invalidUrl = "not-a-valid-url";

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new EmbedBuilder(title, invalidUrl));
		Assert.Contains("URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithFtpUrl_ThrowsArgumentException()
	{
		// Arrange
		var title = "Test Title";
		var ftpUrl = "ftp://example.com";

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new EmbedBuilder(title, ftpUrl));
		Assert.Contains("URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void SetDescription_WithValidDescription_SetsDescription()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var description = "This is a test description";

		// Act
		var result = builder.SetDescription(description);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.Equal(description, embed.Description);
	}

	[Fact]
	public void SetDescription_WithNullDescription_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetDescription(null!));
		Assert.Contains("Description cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("description", exception.ParamName);
	}

	[Fact]
	public void SetDescription_WithEmptyDescription_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetDescription(string.Empty));
		Assert.Contains("Description cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("description", exception.ParamName);
	}

	[Fact]
	public void SetDescription_WithDescriptionExceeding4096Characters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var description = new string('a', 4097);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetDescription(description));
		Assert.Contains("Description cannot exceed 4096 characters", exception.Message);
		Assert.Equal("description", exception.ParamName);
	}

	[Fact]
	public void SetColor_WithColor_SetsColor()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var color = new Color(0xFF0000);

		// Act
		var result = builder.SetColor(color);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.Equal(color, embed.Color);
	}

	[Fact]
	public void SetAuthor_WithName_SetsAuthor()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var authorName = "Test Author";

		// Act
		var result = builder.SetAuthor(authorName);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Author);
		Assert.Equal(authorName, embed.Author!.Name);
		Assert.Null(embed.Author.Url);
		Assert.Null(embed.Author.IconUrl);
	}

	[Fact]
	public void SetAuthor_WithNameAndUrl_SetsAuthorWithUrl()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var authorName = "Test Author";
		var authorUrl = "https://example.com/author";

		// Act
		var result = builder.SetAuthor(authorName, authorUrl);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Author);
		Assert.Equal(authorName, embed.Author!.Name);
		Assert.Equal(authorUrl, embed.Author.Url);
		Assert.Null(embed.Author.IconUrl);
	}

	[Fact]
	public void SetAuthor_WithNameUrlAndIcon_SetsAuthorWithAllProperties()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var authorName = "Test Author";
		var authorUrl = "https://example.com/author";
		var iconUrl = "https://example.com/icon.png";

		// Act
		var result = builder.SetAuthor(authorName, authorUrl, iconUrl);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Author);
		Assert.Equal(authorName, embed.Author!.Name);
		Assert.Equal(authorUrl, embed.Author.Url);
		Assert.Equal(iconUrl, embed.Author.IconUrl);
	}

	[Fact]
	public void SetAuthor_WithNullName_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetAuthor(null!));
		Assert.Contains("Author name cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("name", exception.ParamName);
	}

	[Fact]
	public void SetAuthor_WithNameExceeding256Characters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var name = new string('a', 257);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetAuthor(name));
		Assert.Contains("Author name cannot exceed 256 characters", exception.Message);
		Assert.Equal("name", exception.ParamName);
	}

	[Fact]
	public void SetAuthor_WithInvalidUrl_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetAuthor("Author", "invalid-url"));
		Assert.Contains("URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void SetAuthor_WithInvalidIconUrl_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetAuthor("Author", "https://example.com", "invalid-url"));
		Assert.Contains("Icon URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("iconUrl", exception.ParamName);
	}

	[Fact]
	public void SetThumbnail_WithValidUrl_SetsThumbnail()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var thumbnailUrl = "https://example.com/thumbnail.png";

		// Act
		var result = builder.SetThumbnail(thumbnailUrl);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Thumbnail);
		Assert.Equal(thumbnailUrl, embed.Thumbnail!.Url);
	}

	[Fact]
	public void SetThumbnail_WithInvalidUrl_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetThumbnail("invalid-url"));
		Assert.Contains("Thumbnail URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void AddField_WithValidField_AddsField()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var fieldName = "Field Name";
		var fieldValue = "Field Value";

		// Act
		var result = builder.AddField(fieldName, fieldValue);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.Single(embed.Fields!);
		Assert.Equal(fieldName, embed.Fields![0].Name);
		Assert.Equal(fieldValue, embed.Fields[0].Value);
		Assert.False(embed.Fields[0].Inline);
	}

	[Fact]
	public void AddField_WithInlineTrue_SetsInlineToTrue()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act
		builder.AddField("Name", "Value", inline: true);

		// Assert
		var embed = builder.Build();
		Assert.True(embed.Fields![0].Inline);
	}

	[Fact]
	public void AddField_WithNullName_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.AddField(null!, "Value"));
		Assert.Contains("Field name cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("name", exception.ParamName);
	}

	[Fact]
	public void AddField_WithNameExceeding256Characters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var name = new string('a', 257);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddField(name, "Value"));
		Assert.Contains("Field name cannot exceed 256 characters", exception.Message);
		Assert.Equal("name", exception.ParamName);
	}

	[Fact]
	public void AddField_WithValueExceeding1024Characters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var value = new string('a', 1025);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddField("Name", value));
		Assert.Contains("Field value cannot exceed 1024 characters", exception.Message);
		Assert.Equal("value", exception.ParamName);
	}

	[Fact]
	public void AddField_WithMoreThan25Fields_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		for (int i = 0; i < 25; i++)
		{
			builder.AddField($"Field {i}", $"Value {i}");
		}

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => builder.AddField("Field 26", "Value 26"));
		Assert.Contains("Embed cannot have more than 25 fields", exception.Message);
	}

	[Fact]
	public void SetImage_WithValidUrl_SetsImage()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var imageUrl = "https://example.com/image.png";

		// Act
		var result = builder.SetImage(imageUrl);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Image);
		Assert.Equal(imageUrl, embed.Image!.Url);
	}

	[Fact]
	public void SetImage_WithInvalidUrl_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetImage("invalid-url"));
		Assert.Contains("Image URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void SetFooter_WithText_SetsFooter()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var footerText = "Footer Text";

		// Act
		var result = builder.SetFooter(footerText);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Footer);
		Assert.Equal(footerText, embed.Footer!.Text);
		Assert.Null(embed.Footer.IconUrl);
	}

	[Fact]
	public void SetFooter_WithTextAndIcon_SetsFooterWithIcon()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var footerText = "Footer Text";
		var iconUrl = "https://example.com/icon.png";

		// Act
		var result = builder.SetFooter(footerText, iconUrl);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Footer);
		Assert.Equal(footerText, embed.Footer!.Text);
		Assert.Equal(iconUrl, embed.Footer.IconUrl);
	}

	[Fact]
	public void SetFooter_WithTextExceeding2048Characters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var text = new string('a', 2049);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetFooter(text));
		Assert.Contains("Footer text cannot exceed 2048 characters", exception.Message);
		Assert.Equal("text", exception.ParamName);
	}

	[Fact]
	public void SetFooter_WithInvalidIconUrl_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetFooter("Footer", "invalid-url"));
		Assert.Contains("Icon URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("iconUrl", exception.ParamName);
	}

	[Fact]
	public void SetTimestamp_WithNoParameters_SetsCurrentTime()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var beforeTime = DateTimeOffset.UtcNow;

		// Act
		builder.SetTimestamp();

		// Assert
		var embed = builder.Build();
		Assert.NotNull(embed.Timestamp);
		var timestamp = DateTimeOffset.Parse(embed.Timestamp!);
		var afterTime = DateTimeOffset.UtcNow;
		Assert.True(timestamp >= beforeTime.AddSeconds(-1));
		Assert.True(timestamp <= afterTime.AddSeconds(1));
	}

	[Fact]
	public void SetTimestamp_WithDateTimeOffset_SetsTimestamp()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var timestamp = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

		// Act
		builder.SetTimestamp(timestamp);

		// Assert
		var embed = builder.Build();
		Assert.NotNull(embed.Timestamp);
		Assert.Equal("2024-01-01T12:00:00.000Z", embed.Timestamp);
	}

	[Fact]
	public void SetTimestamp_WithDateTime_SetsTimestamp()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

		// Act
		builder.SetTimestamp(timestamp);

		// Assert
		var embed = builder.Build();
		Assert.NotNull(embed.Timestamp);
		Assert.Contains("2024-01-01", embed.Timestamp);
	}

	[Fact]
	public void SetType_WithType_SetsType()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var type = "rich";

		// Act
		var result = builder.SetType(type);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.Equal(type, embed.Type);
	}

	[Fact]
	public void SetType_WithNullType_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetType(null!));
		Assert.Contains("Type cannot be null, empty, or contain only whitespace", exception.Message);
		Assert.Equal("type", exception.ParamName);
	}

	[Fact]
	public void SetVideo_WithUrl_SetsVideo()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var videoUrl = "https://example.com/video.mp4";

		// Act
		var result = builder.SetVideo(videoUrl);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Video);
		Assert.Equal(videoUrl, embed.Video!.Url);
		Assert.Null(embed.Video.Width);
		Assert.Null(embed.Video.Height);
	}

	[Fact]
	public void SetVideo_WithUrlAndDimensions_SetsVideoWithDimensions()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var videoUrl = "https://example.com/video.mp4";
		var width = 1920;
		var height = 1080;

		// Act
		var result = builder.SetVideo(videoUrl, width, height);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Video);
		Assert.Equal(videoUrl, embed.Video!.Url);
		Assert.Equal(width, embed.Video.Width);
		Assert.Equal(height, embed.Video.Height);
	}

	[Fact]
	public void SetVideo_WithInvalidUrl_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetVideo("invalid-url"));
		Assert.Contains("Video URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void SetVideo_WithNegativeWidth_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetVideo("https://example.com/video.mp4", -1, 1080));
		Assert.Contains("Width cannot be negative", exception.Message);
		Assert.Equal("width", exception.ParamName);
	}

	[Fact]
	public void SetVideo_WithNegativeHeight_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetVideo("https://example.com/video.mp4", 1920, -1));
		Assert.Contains("Height cannot be negative", exception.Message);
		Assert.Equal("height", exception.ParamName);
	}

	[Fact]
	public void SetProvider_WithName_SetsProvider()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var providerName = "Test Provider";

		// Act
		var result = builder.SetProvider(providerName);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Provider);
		Assert.Equal(providerName, embed.Provider!.Name);
		Assert.Null(embed.Provider.Url);
	}

	[Fact]
	public void SetProvider_WithNameAndUrl_SetsProviderWithUrl()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var providerName = "Test Provider";
		var providerUrl = "https://example.com";

		// Act
		var result = builder.SetProvider(providerName, providerUrl);

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed.Provider);
		Assert.Equal(providerName, embed.Provider!.Name);
		Assert.Equal(providerUrl, embed.Provider.Url);
	}

	[Fact]
	public void SetProvider_WithNameExceeding256Characters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		var name = new string('a', 257);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.SetProvider(name));
		Assert.Contains("Provider name cannot exceed 256 characters", exception.Message);
		Assert.Equal("name", exception.ParamName);
	}

	[Fact]
	public void SetProvider_WithInvalidUrl_ThrowsArgumentException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.SetProvider("Provider", "invalid-url"));
		Assert.Contains("Provider URL must be a valid HTTP or HTTPS URL", exception.Message);
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void Build_WithTotalLengthExceeding6000Characters_ThrowsInvalidOperationException()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");
		builder.SetDescription(new string('a', 3000));
		for (int i = 0; i < 10; i++)
		{
			builder.AddField(new string('a', 200), new string('a', 300));
		}

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
		Assert.Contains("Total embed length", exception.Message);
		Assert.Contains("cannot exceed 6000 characters", exception.Message);
	}

	[Fact]
	public void Build_WithValidEmbed_ReturnsEmbed()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title")
			.SetDescription("Test Description")
			.SetColor(new Color(0xFF0000))
			.AddField("Field 1", "Value 1")
			.AddField("Field 2", "Value 2", inline: true);

		// Act
		var embed = builder.Build();

		// Assert
		Assert.NotNull(embed);
		Assert.Equal("Test Title", embed.Title);
		Assert.Equal("Test Description", embed.Description);
		Assert.NotNull(embed.Color);
		Assert.Equal(2, embed.Fields!.Length);
	}

	[Fact]
	public void FluentInterface_AllowsMethodChaining()
	{
		// Arrange
		var builder = new EmbedBuilder("Test Title");

		// Act
		var result = builder
			.SetDescription("Description")
			.SetColor(new Color(0x00FF00))
			.SetAuthor("Author", "https://example.com")
			.AddField("Field", "Value")
			.SetFooter("Footer")
			.SetThumbnail("https://example.com/thumb.png")
			.SetImage("https://example.com/image.png");

		// Assert
		Assert.Same(builder, result);
		var embed = builder.Build();
		Assert.NotNull(embed);
		Assert.Equal("Description", embed.Description);
		Assert.NotNull(embed.Author);
		Assert.NotNull(embed.Footer);
		Assert.NotNull(embed.Thumbnail);
		Assert.NotNull(embed.Image);
	}
}
