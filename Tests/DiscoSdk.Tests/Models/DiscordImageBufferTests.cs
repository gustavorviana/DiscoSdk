using DiscoSdk.Models;

namespace DiscoSdk.Tests.Models;

public class DiscordImageBufferTests
{
	[Fact]
	public void Constructor_WithBufferAndExtension_CreatesInstance()
	{
		// Arrange
		var buffer = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
		var extension = "png";

		// Act
		var imageBuffer = new DiscordImageBuffer(buffer, extension);

		// Assert
		Assert.Equal(buffer, imageBuffer.Buffer);
		Assert.Equal(extension, imageBuffer.Extension);
		Assert.Equal($"image/{extension}", imageBuffer.ImageType);
	}

	[Fact]
	public void Constructor_WithBufferOnly_DetectsPngExtension()
	{
		// Arrange
		var buffer = new byte[]
		{
			0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
			0x00, 0x00, 0x00, 0x00
		};

		// Act
		var imageBuffer = new DiscordImageBuffer(buffer);

		// Assert
		Assert.Equal(buffer, imageBuffer.Buffer);
		Assert.Equal("png", imageBuffer.Extension);
		Assert.Equal("image/png", imageBuffer.ImageType);
	}

	[Fact]
	public void Constructor_WithBufferOnly_DetectsJpegExtension()
	{
		// Arrange
		var buffer = new byte[]
		{
			0xFF, 0xD8, // JPEG signature
			0xFF, 0xE0, 0x00, 0x10
		};

		// Act
		var imageBuffer = new DiscordImageBuffer(buffer);

		// Assert
		Assert.Equal(buffer, imageBuffer.Buffer);
		Assert.Equal("jpeg", imageBuffer.Extension);
		Assert.Equal("image/jpeg", imageBuffer.ImageType);
	}

	[Fact]
	public void Constructor_WithBufferOnly_DetectsGifExtension()
	{
		// Arrange
		var buffer = new byte[]
		{
			0x47, 0x49, 0x46, // GIF signature
			0x38, 0x39, 0x61
		};

		// Act
		var imageBuffer = new DiscordImageBuffer(buffer);

		// Assert
		Assert.Equal(buffer, imageBuffer.Buffer);
		Assert.Equal("gif", imageBuffer.Extension);
		Assert.Equal("image/gif", imageBuffer.ImageType);
	}

	[Fact]
	public void Constructor_WithBufferOnly_WithUnsupportedFormat_ThrowsInvalidOperationException()
	{
		// Arrange
		var buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => new DiscordImageBuffer(buffer));
		Assert.Equal("Unsupported image format. Only PNG, JPEG and GIF are allowed.", exception.Message);
	}

	[Fact]
	public void Constructor_WithBufferOnly_WithEmptyBuffer_ThrowsInvalidOperationException()
	{
		// Arrange
		var buffer = Array.Empty<byte>();

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => new DiscordImageBuffer(buffer));
		Assert.Equal("Unsupported image format. Only PNG, JPEG and GIF are allowed.", exception.Message);
	}

	[Fact]
	public void Constructor_WithBufferOnly_WithSingleByte_ThrowsInvalidOperationException()
	{
		// Arrange
		var buffer = new byte[] { 0xFF };

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => new DiscordImageBuffer(buffer));
		Assert.Equal("Unsupported image format. Only PNG, JPEG and GIF are allowed.", exception.Message);
	}

	[Fact]
	public void Constructor_WithBufferOnly_WithTwoBytesNotJpeg_ThrowsInvalidOperationException()
	{
		// Arrange
		var buffer = new byte[] { 0xFF, 0xD9 }; // Not JPEG (JPEG is 0xFF 0xD8)

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => new DiscordImageBuffer(buffer));
		Assert.Equal("Unsupported image format. Only PNG, JPEG and GIF are allowed.", exception.Message);
	}

	[Fact]
	public void Constructor_WithBufferOnly_WithThreeBytesNotGif_ThrowsInvalidOperationException()
	{
		// Arrange
		var buffer = new byte[] { 0x47, 0x49, 0x47 }; // Not GIF (GIF is 0x47 0x49 0x46)

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => new DiscordImageBuffer(buffer));
		Assert.Equal("Unsupported image format. Only PNG, JPEG and GIF are allowed.", exception.Message);
	}

	[Fact]
	public void Constructor_WithBufferOnly_WithLessThan8BytesNotPng_ThrowsInvalidOperationException()
	{
		// Arrange
		var buffer = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A }; // 7 bytes, missing last byte

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => new DiscordImageBuffer(buffer));
		Assert.Equal("Unsupported image format. Only PNG, JPEG and GIF are allowed.", exception.Message);
	}

	[Fact]
	public void Constructor_WithBufferOnly_With8BytesNotPng_ThrowsInvalidOperationException()
	{
		// Arrange
		var buffer = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x00 }; // Wrong last byte

		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => new DiscordImageBuffer(buffer));
		Assert.Equal("Unsupported image format. Only PNG, JPEG and GIF are allowed.", exception.Message);
	}

	[Fact]
	public void Constructor_WithExplicitExtension_OverridesDetection()
	{
		// Arrange
		var buffer = new byte[] { 0xFF, 0xD8 }; // JPEG signature
		var extension = "custom";

		// Act
		var imageBuffer = new DiscordImageBuffer(buffer, extension);

		// Assert
		Assert.Equal(buffer, imageBuffer.Buffer);
		Assert.Equal(extension, imageBuffer.Extension);
		Assert.Equal($"image/{extension}", imageBuffer.ImageType);
	}

	[Fact]
	public void Constructor_WithEmptyExtension_DetectsFromBuffer()
	{
		// Arrange
		var buffer = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

		// Act
		var imageBuffer = new DiscordImageBuffer(buffer, string.Empty);

		// Assert
		Assert.Equal("jpeg", imageBuffer.Extension);
	}

	[Fact]
	public void Constructor_WithNullExtension_DetectsFromBuffer()
	{
		// Arrange
		var buffer = new byte[] { 0x47, 0x49, 0x46, 0x38 };

		// Act
		var imageBuffer = new DiscordImageBuffer(buffer, null);

		// Assert
		Assert.Equal("gif", imageBuffer.Extension);
	}

	[Fact]
	public void ToBase64_WithBuffer_ReturnsBase64String()
	{
		// Arrange
		var buffer = new byte[] { 0x01, 0x02, 0x03, 0x04 };
		var imageBuffer = new DiscordImageBuffer(buffer, "png");
		var expectedBase64 = Convert.ToBase64String(buffer);

		// Act
		var result = imageBuffer.ToBase64();

		// Assert
		Assert.Equal(expectedBase64, result);
	}

	[Fact]
	public void ToBase64_WithEmptyBuffer_ReturnsEmptyBase64String()
	{
		// Arrange
		var buffer = Array.Empty<byte>();
		var imageBuffer = new DiscordImageBuffer(buffer, "png");

		// Act
		var result = imageBuffer.ToBase64();

		// Assert
		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void FromBase64_WithValidBase64_ReturnsDiscordImageBuffer()
	{
		// Arrange
		var originalBuffer = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
		var base64 = Convert.ToBase64String(originalBuffer);

		// Act
		var result = DiscordImageBuffer.FromBase64(base64);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(originalBuffer, result.Buffer);
		Assert.Equal("png", result.Extension);
	}

	[Fact]
	public void FromBase64_WithNull_ReturnsNull()
	{
		// Act
		var result = DiscordImageBuffer.FromBase64(null);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void FromBase64_WithEmptyString_ReturnsNull()
	{
		// Act
		var result = DiscordImageBuffer.FromBase64(string.Empty);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void FromBase64_WithJpegBase64_DetectsJpegExtension()
	{
		// Arrange
		var jpegBuffer = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
		var base64 = Convert.ToBase64String(jpegBuffer);

		// Act
		var result = DiscordImageBuffer.FromBase64(base64);

		// Assert
		Assert.NotNull(result);
		Assert.Equal("jpeg", result.Extension);
	}

	[Fact]
	public void FromBase64_WithGifBase64_DetectsGifExtension()
	{
		// Arrange
		var gifBuffer = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
		var base64 = Convert.ToBase64String(gifBuffer);

		// Act
		var result = DiscordImageBuffer.FromBase64(base64);

		// Assert
		Assert.NotNull(result);
		Assert.Equal("gif", result.Extension);
	}

	[Fact]
	public void FromBase64_WithInvalidBase64_ThrowsFormatException()
	{
		// Arrange
		var invalidBase64 = "!!!invalid base64!!!";

		// Act & Assert
		Assert.Throws<FormatException>(() => DiscordImageBuffer.FromBase64(invalidBase64));
	}

	[Fact]
	public void LoadFile_WithExistingFile_LoadsFile()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			var pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
			File.WriteAllBytes(tempFile, pngSignature);

			// Act
			var result = DiscordImageBuffer.LoadFile(tempFile);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(pngSignature, result.Buffer);
			Assert.Equal("png", result.Extension);
		}
		finally
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}
	}

	[Fact]
	public void LoadFile_WithNonExistentFile_ThrowsFileNotFoundException()
	{
		// Arrange
		var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

		// Act & Assert
		Assert.Throws<FileNotFoundException>(() => DiscordImageBuffer.LoadFile(nonExistentFile));
	}

	[Fact]
	public void RoundTrip_ToBase64AndFromBase64_ReturnsSameBuffer()
	{
		// Arrange
		var originalBuffer = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
		var imageBuffer = new DiscordImageBuffer(originalBuffer, "png");

		// Act
		var base64 = imageBuffer.ToBase64();
		var result = DiscordImageBuffer.FromBase64(base64);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(originalBuffer, result.Buffer);
		Assert.Equal("png", result.Extension);
	}

	[Fact]
	public void Buffer_IsReadOnly_ReturnsSameReference()
	{
		// Arrange
		var buffer = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
		var imageBuffer = new DiscordImageBuffer(buffer, "png");

		// Act
		var retrievedBuffer = imageBuffer.Buffer;

		// Assert
		Assert.Same(buffer, retrievedBuffer);
	}
}

