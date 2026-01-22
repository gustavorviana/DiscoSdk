using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.JsonConverters;

public class ApplicationCommandCountMapConverterTests
{
	private readonly ApplicationCommandCountMapConverter _converter = new();
	private readonly JsonSerializerOptions _options = new();

	[Fact]
	public void Read_WithNull_ReturnsEmptyDictionary()
	{
		// Arrange
		var json = "null";
		var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

		// Act
		var result = _converter.Read(ref reader, typeof(Dictionary<ApplicationCommandType, int>), _options);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public void Read_WithEmptyObject_ReturnsEmptyDictionary()
	{
		// Arrange
		var json = "{}";
		var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

		// Act
		var result = _converter.Read(ref reader, typeof(Dictionary<ApplicationCommandType, int>), _options);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public void Write_WithNull_WritesNull()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, null!, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("null", json);
	}

	[Fact]
	public void Write_WithValidDictionary_SerializesCorrectly()
	{
		// Arrange
		var dict = new Dictionary<ApplicationCommandType, int>
		{
			{ ApplicationCommandType.ChatInput, 5 },
			{ ApplicationCommandType.User, 3 },
			{ ApplicationCommandType.Message, 2 }
		};
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, dict, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("\"chat_input\":5", json);
		Assert.Contains("\"user\":3", json);
		Assert.Contains("\"message\":2", json);
	}

	[Fact]
	public void Write_WithEmptyDictionary_SerializesEmptyObject()
	{
		// Arrange
		var dict = new Dictionary<ApplicationCommandType, int>();
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, dict, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("{}", json);
	}

	[Fact]
	public void Write_WithInvalidEnumValue_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var dict = new Dictionary<ApplicationCommandType, int>
		{
			{ (ApplicationCommandType)999, 5 }
		};
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			_converter.Write(writer, dict, _options));
	}
}