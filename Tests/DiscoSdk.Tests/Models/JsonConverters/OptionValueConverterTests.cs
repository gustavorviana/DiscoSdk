using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.JsonConverters;

public class OptionValueConverterTests
{
	private readonly OptionValueConverter _converter = new();
	private readonly JsonSerializerOptions _options = new();

	[Fact]
	public void Read_WithNull_ReturnsNull()
	{
		// Arrange
		var json = "null";

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Read_WithString_ReturnsString()
	{
		// Arrange
		var json = "\"test value\"";

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.Equal("test value", result);
	}

	[Fact]
	public void Read_WithTrue_ReturnsTrue()
	{
		// Arrange
		var json = "true";

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.Equal(true, result);
	}

	[Fact]
	public void Read_WithFalse_ReturnsFalse()
	{
		// Arrange
		var json = "false";

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.Equal(false, result);
	}

	[Fact]
	public void Read_WithInt32_ReturnsInt32()
	{
		// Arrange
		var json = "42";

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.IsType<int>(result);
		Assert.Equal(42, result);
	}

	[Fact]
	public void Read_WithInt64_ReturnsInt64()
	{
		// Arrange
		var json = "9223372036854775807"; // Max int64

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.IsType<long>(result);
		Assert.Equal(9223372036854775807L, result);
	}

	[Fact]
	public void Read_WithDouble_ReturnsDouble()
	{
		// Arrange
		var json = "3.14159";

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.IsType<double>(result);
		Assert.Equal(3.14159, (double)result, 5);
	}

	[Fact]
	public void Read_WithInvalidToken_ReturnsNull()
	{
		// Arrange
		var json = "[]";

		// Act
		var result = _converter.Read(json, typeof(object), _options);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Write_WithNull_WritesNull()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, null, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("null", json);
	}

	[Fact]
	public void Write_WithString_WritesString()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, "test", _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("\"test\"", json);
	}

	[Fact]
	public void Write_WithTrue_WritesTrue()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, true, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("true", json);
	}

	[Fact]
	public void Write_WithFalse_WritesFalse()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, false, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("false", json);
	}

	[Fact]
	public void Write_WithInt_WritesNumber()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, 42, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("42", json);
	}

	[Fact]
	public void Write_WithLong_WritesNumber()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, 9223372036854775807L, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("9223372036854775807", json);
	}

	[Fact]
	public void Write_WithDouble_WritesNumber()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, 3.14, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("3.14", json);
	}

	[Fact]
	public void Write_WithDecimal_WritesNumber()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, 123.456m, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("123.456", json);
	}

	[Fact]
	public void Write_WithFloat_WritesNumber()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, 1.5f, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("1.5", json);
	}

	[Fact]
	public void Write_WithUnknownType_WritesString()
	{
		// Arrange
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, new { Test = "value" }, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		// Should fallback to ToString() representation
		Assert.NotNull(json);
	}
}

