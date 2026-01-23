using DiscoSdk.Models;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.JsonConverters;

public class SnowflakeConverterTests
{
	private readonly SnowflakeConverter _converter = new();
	private readonly JsonSerializerOptions _options = new();

	[Fact]
	public void Read_WithNull_ReturnsDefault()
	{
		// Arrange
		var json = "null";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(default(Snowflake), result);
		Assert.True(result.Empty);
	}

	[Fact]
	public void Read_WithString_ReturnsSnowflake()
	{
		// Arrange
		var json = "\"123456789012345678\"";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(123456789012345678UL, result.Value);
	}

	[Fact]
	public void Read_WithEmptyString_ReturnsDefault()
	{
		// Arrange
		var json = "\"\"";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(default(Snowflake), result);
		Assert.True(result.Empty);
	}

	[Fact]
	public void Read_WithWhitespaceString_ReturnsDefault()
	{
		// Arrange
		var json = "\"   \"";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(default(Snowflake), result);
		Assert.True(result.Empty);
	}

	[Fact]
	public void Read_WithInvalidString_ReturnsDefault()
	{
		// Arrange
		var json = "\"invalid\"";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(default(Snowflake), result);
		Assert.True(result.Empty);
	}

	[Fact]
	public void Read_WithNumber_ReturnsSnowflake()
	{
		// Arrange
		var json = "123456789012345678";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(123456789012345678UL, result.Value);
	}

	[Fact]
	public void Read_WithZero_ReturnsDefault()
	{
		// Arrange
		var json = "0";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(default(Snowflake), result);
		Assert.True(result.Empty);
	}

	[Fact]
	public void Read_WithMaxUInt64_ReturnsMaxSnowflake()
	{
		// Arrange
		var json = "18446744073709551615"; // Max ulong

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(18446744073709551615UL, result.Value);
	}

	[Fact]
	public void Read_WithArray_ReturnsDefault()
	{
		// Arrange
		var json = "[]";

		// Act
		var result = _converter.Read(json, typeof(Snowflake), _options);

		// Assert
		Assert.Equal(default(Snowflake), result);
	}

	[Fact]
	public void Write_WithSnowflake_WritesString()
	{
		// Arrange
		var snowflake = new Snowflake(123456789012345678UL);
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, snowflake, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("\"123456789012345678\"", json);
	}

	[Fact]
	public void Write_WithDefaultSnowflake_WritesZeroString()
	{
		// Arrange
		var snowflake = default(Snowflake);
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, snowflake, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("\"0\"", json);
	}

	[Fact]
	public void Write_WithMaxSnowflake_WritesMaxString()
	{
		// Arrange
		var snowflake = new Snowflake(18446744073709551615UL);
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, snowflake, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("\"18446744073709551615\"", json);
	}
}

