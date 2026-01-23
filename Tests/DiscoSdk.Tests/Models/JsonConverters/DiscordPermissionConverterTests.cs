using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.JsonConverters;

public class DiscordPermissionConverterTests
{
	private readonly DiscordPermissionConverter _converter = new();
	private readonly JsonSerializerOptions _options = new();

	[Fact]
	public void Read_WithNumber_ReturnsPermission()
	{
		// Act
		var result = _converter.Read("8", typeof(DiscordPermission), _options);

		// Assert
		Assert.Equal(DiscordPermission.Administrator, result);
	}

	[Fact]
	public void Read_WithString_ReturnsPermission()
	{
        // Act
        var result = _converter.Read("\"8\"", typeof(DiscordPermission), _options);

		// Assert
		Assert.Equal(DiscordPermission.Administrator, result);
	}

	[Fact]
	public void Read_WithZero_ReturnsNone()
	{
        var result = _converter.Read("0", typeof(DiscordPermission), _options);

		// Assert
		Assert.Equal(DiscordPermission.None, result);
	}

	[Fact]
	public void Read_WithLargeNumber_ReturnsPermission()
	{
        var result = _converter.Read("8589934592", typeof(DiscordPermission), _options);

		// Assert
		Assert.Equal((DiscordPermission)8589934592UL, result);
	}

	[Fact]
	public void Read_WithInvalidString_ThrowsJsonException()
	{
		// Arrange
		var json = "\"invalid\"";
		var bytes = System.Text.Encoding.UTF8.GetBytes(json);

		// Act & Assert
		var exception = Assert.Throws<JsonException>(() =>
		{
			var reader = new Utf8JsonReader(bytes);
			reader.Read(); // Posiciona o reader no primeiro token
			_converter.Read(ref reader, typeof(DiscordPermission), _options);
		});
		Assert.Contains("Invalid Discord permission value", exception.Message);
	}

	[Fact]
	public void Read_WithNull_ThrowsJsonException()
	{
		// Arrange
		var json = "null";
		var bytes = System.Text.Encoding.UTF8.GetBytes(json);

		// Act & Assert
		var exception = Assert.Throws<JsonException>(() =>
		{
			var reader = new Utf8JsonReader(bytes);
			reader.Read(); // Posiciona o reader no primeiro token
			_converter.Read(ref reader, typeof(DiscordPermission), _options);
		});
		Assert.Contains("Invalid Discord permission value", exception.Message);
	}

	[Fact]
	public void Read_WithArray_ThrowsJsonException()
	{
		// Arrange
		var json = "[]";
		var bytes = System.Text.Encoding.UTF8.GetBytes(json);

		// Act & Assert
		var exception = Assert.Throws<JsonException>(() =>
		{
			var reader = new Utf8JsonReader(bytes);
			reader.Read(); // Posiciona o reader no primeiro token
			_converter.Read(ref reader, typeof(DiscordPermission), _options);
		});
		Assert.Contains("Invalid Discord permission value", exception.Message);
	}

	[Fact]
	public void Write_WithPermission_WritesString()
	{
		// Arrange
		var permission = DiscordPermission.Administrator;
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, permission, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("\"8\"", json);
	}

	[Fact]
	public void Write_WithNone_WritesZeroString()
	{
		// Arrange
		var permission = DiscordPermission.None;
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, permission, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("\"0\"", json);
	}

	[Fact]
	public void Write_WithCombinedPermissions_WritesCorrectValue()
	{
		// Arrange
		var permission = DiscordPermission.SendMessages | DiscordPermission.ViewChannel;
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, permission, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		var expectedValue = ((ulong)DiscordPermission.SendMessages | (ulong)DiscordPermission.ViewChannel).ToString();
		Assert.Equal($"\"{expectedValue}\"", json);
	}
}

