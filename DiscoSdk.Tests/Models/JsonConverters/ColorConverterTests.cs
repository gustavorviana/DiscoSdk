using DiscoSdk.Models;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.JsonConverters;

public class ColorConverterTests
{
	private readonly ColorConverter _converter = new();
	private readonly JsonSerializerOptions _options = new();

	[Fact]
	public void Read_WithNull_ReturnsDefault()
	{
		// Act
		var result = _converter.Read("null", typeof(Color), _options);

		// Assert
		Assert.Equal(default, result);
	}

	[Fact]
	public void Read_WithNumber_ReturnsColor()
	{
		// Act
		var result = _converter.Read("16711680", typeof(Color), _options);

		// Assert
		Assert.Equal(16711680, result.Value);
	}

	[Fact]
	public void Read_WithZero_ReturnsDefaultColor()
	{
		// Act
		var result = _converter.Read("0", typeof(Color), _options);

		// Assert
		Assert.Equal(0, result.Value);
	}

	[Fact]
	public void Read_WithMaxValue_ReturnsMaxColor()
	{
		var result = _converter.Read("16777215", typeof(Color), _options);

		// Assert
		Assert.Equal(16777215, result.Value);
	}

	[Fact]
	public void Read_WithNonNumber_ReturnsDefault()
	{
		// Act
		var result = _converter.Read("\"invalid\"", typeof(Color), _options);

		// Assert
		Assert.Equal(default, result);
	}

	[Fact]
	public void Write_WithColor_WritesNumber()
	{
		// Arrange
		var color = new Color(16711680); // Red
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, color, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("16711680", json);
	}

	[Fact]
	public void Write_WithDefaultColor_WritesZero()
	{
		// Arrange
		var color = default(Color);
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, color, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("0", json);
	}

	[Fact]
	public void Write_WithMaxColor_WritesMaxValue()
	{
		// Arrange
		var color = new Color(16777215); // White
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, color, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("16777215", json);
	}
}

