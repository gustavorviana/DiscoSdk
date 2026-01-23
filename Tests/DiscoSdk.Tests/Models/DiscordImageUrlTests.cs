using DiscoSdk.Models;

namespace DiscoSdk.Tests.Models;

public class DiscordImageUrlTests
{
	[Fact]
	public void Constructor_WithUrlAndExtension_CreatesInstance()
	{
		// Arrange
		var url = "https://example.com/image.png";
		var extension = "png";

		// Act
		var imageUrl = new DiscordImageUrl(url, extension);

		// Assert
		Assert.Equal(url, imageUrl.Url);
		Assert.Equal(extension, imageUrl.Extension);
		Assert.Equal($"image/{extension}", imageUrl.ImageType);
	}

	[Fact]
	public void ParseAvatar_WithNullHash_ReturnsDefaultAvatar()
	{
		// Arrange
		var userId = new Snowflake(123456789012345678UL);

		// Act
		var result = DiscordImageUrl.ParseAvatar(userId, null);

		// Assert
		Assert.NotNull(result);
		var expectedMod = userId.Value % 5;
		Assert.Equal($"https://cdn.discordapp.com/embed/avatars/{expectedMod}.png", result.Url);
		Assert.Equal("png", result.Extension);
		Assert.Equal("image/png", result.ImageType);
	}

	[Fact]
	public void ParseAvatar_WithHash_ReturnsAvatarUrl()
	{
		// Arrange
		var userId = new Snowflake(123456789012345678UL);
		var avatarHash = "abc123def456";

		// Act
		var result = DiscordImageUrl.ParseAvatar(userId, avatarHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png", result.Url);
		Assert.Equal("png", result.Extension);
	}

	[Fact]
	public void ParseAvatar_WithAnimatedHash_ReturnsGifUrl()
	{
		// Arrange
		var userId = new Snowflake(123456789012345678UL);
		var avatarHash = "a_abc123def456";

		// Act
		var result = DiscordImageUrl.ParseAvatar(userId, avatarHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.gif", result.Url);
		Assert.Equal("gif", result.Extension);
		Assert.Equal("image/gif", result.ImageType);
	}

	[Fact]
	public void ParseBanner_WithNullHash_ReturnsNull()
	{
		// Arrange
		var userId = new Snowflake(123456789012345678UL);

		// Act
		var result = DiscordImageUrl.ParseBanner(userId, null);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseBanner_WithHash_ReturnsBannerUrl()
	{
		// Arrange
		var userId = new Snowflake(123456789012345678UL);
		var bannerHash = "banner123";

		// Act
		var result = DiscordImageUrl.ParseBanner(userId, bannerHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/banners/{userId}/{bannerHash}.png", result.Url);
		Assert.Equal("png", result.Extension);
	}

	[Fact]
	public void ParseBanner_WithAnimatedHash_ReturnsGifUrl()
	{
		// Arrange
		var userId = new Snowflake(123456789012345678UL);
		var bannerHash = "a_banner123";

		// Act
		var result = DiscordImageUrl.ParseBanner(userId, bannerHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/banners/{userId}/{bannerHash}.gif", result.Url);
		Assert.Equal("gif", result.Extension);
	}

	[Fact]
	public void ParseIcon_WithNullHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseIcon(guildId, null);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseIcon_WithEmptyHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseIcon(guildId, string.Empty);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseIcon_WithWhitespaceHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseIcon(guildId, "   ");

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseIcon_WithHash_ReturnsIconUrl()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);
		var iconHash = "icon123";

		// Act
		var result = DiscordImageUrl.ParseIcon(guildId, iconHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/icons/{guildId}/{iconHash}.png", result.Url);
		Assert.Equal("png", result.Extension);
	}

	[Fact]
	public void ParseIcon_WithAnimatedHash_ReturnsGifUrl()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);
		var iconHash = "a_icon123";

		// Act
		var result = DiscordImageUrl.ParseIcon(guildId, iconHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/icons/{guildId}/{iconHash}.gif", result.Url);
		Assert.Equal("gif", result.Extension);
	}

	[Fact]
	public void ParseSplash_WithNullHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseSplash(guildId, null);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseSplash_WithEmptyHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseSplash(guildId, string.Empty);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseSplash_WithWhitespaceHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseSplash(guildId, "   ");

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseSplash_WithHash_ReturnsSplashUrl()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);
		var splashHash = "splash123";

		// Act
		var result = DiscordImageUrl.ParseSplash(guildId, splashHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/splashes/{guildId}/{splashHash}.png", result.Url);
		Assert.Equal("png", result.Extension);
	}

	[Fact]
	public void ParseSplash_WithAnimatedHash_ReturnsGifUrl()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);
		var splashHash = "a_splash123";

		// Act
		var result = DiscordImageUrl.ParseSplash(guildId, splashHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/splashes/{guildId}/{splashHash}.gif", result.Url);
		Assert.Equal("gif", result.Extension);
	}

	[Fact]
	public void ParseDiscoverySplash_WithNullHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseDiscoverySplash(guildId, null);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseDiscoverySplash_WithEmptyHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseDiscoverySplash(guildId, string.Empty);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseDiscoverySplash_WithWhitespaceHash_ReturnsNull()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);

		// Act
		var result = DiscordImageUrl.ParseDiscoverySplash(guildId, "   ");

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void ParseDiscoverySplash_WithHash_ReturnsDiscoverySplashUrl()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);
		var discoverySplashHash = "discovery123";

		// Act
		var result = DiscordImageUrl.ParseDiscoverySplash(guildId, discoverySplashHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/discovery-splashes/{guildId}/{discoverySplashHash}.png", result.Url);
		Assert.Equal("png", result.Extension);
	}

	[Fact]
	public void ParseDiscoverySplash_WithAnimatedHash_ReturnsGifUrl()
	{
		// Arrange
		var guildId = new Snowflake(987654321098765432UL);
		var discoverySplashHash = "a_discovery123";

		// Act
		var result = DiscordImageUrl.ParseDiscoverySplash(guildId, discoverySplashHash);

		// Assert
		Assert.NotNull(result);
		Assert.Equal($"https://cdn.discordapp.com/discovery-splashes/{guildId}/{discoverySplashHash}.gif", result.Url);
		Assert.Equal("gif", result.Extension);
	}

	[Fact]
	public void ParseAvatar_WithDifferentUserIds_ReturnsCorrectDefaultAvatars()
	{
		// Arrange & Act & Assert
		var userId0 = new Snowflake(0UL);
		var result0 = DiscordImageUrl.ParseAvatar(userId0, null);
		Assert.Equal("https://cdn.discordapp.com/embed/avatars/0.png", result0.Url);

		var userId1 = new Snowflake(1UL);
		var result1 = DiscordImageUrl.ParseAvatar(userId1, null);
		Assert.Equal("https://cdn.discordapp.com/embed/avatars/1.png", result1.Url);

		var userId4 = new Snowflake(4UL);
		var result4 = DiscordImageUrl.ParseAvatar(userId4, null);
		Assert.Equal("https://cdn.discordapp.com/embed/avatars/4.png", result4.Url);

		var userId5 = new Snowflake(5UL);
		var result5 = DiscordImageUrl.ParseAvatar(userId5, null);
		Assert.Equal("https://cdn.discordapp.com/embed/avatars/0.png", result5.Url); // 5 % 5 = 0

		var userId10 = new Snowflake(10UL);
		var result10 = DiscordImageUrl.ParseAvatar(userId10, null);
		Assert.Equal("https://cdn.discordapp.com/embed/avatars/0.png", result10.Url); // 10 % 5 = 0
	}
}

