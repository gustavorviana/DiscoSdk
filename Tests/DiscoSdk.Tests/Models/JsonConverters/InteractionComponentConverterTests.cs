using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;

namespace DiscoSdk.Tests.Models.JsonConverters;

public class InteractionComponentConverterTests
{
	private readonly InteractionComponentConverter _converter = new();
	private readonly JsonSerializerOptions _options = new();

	[Fact]
	public void Read_WithNull_ReturnsNull()
	{
		// Arrange
		var json = "null";

		// Act
		var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void Read_WithActionRow_DeserializesActionRowComponent()
	{
        // Arrange
        var json = """[{"type":1,"components":[]}]""";
        // Act
        var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result);
		var component = Assert.IsType<ActionRowComponent>(result[0]);
		Assert.Equal(ComponentType.ActionRow, component.Type);
	}

	[Fact]
	public void Read_WithButton_DeserializesMessageComponent()
	{
		// Arrange
		var json = """[{"type":2,"custom_id":"test","label":"Click Me"}]""";

		// Act
		var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result);
		Assert.IsType<MessageComponent>(result[0]);
		var component = (MessageComponent)result[0];
		Assert.Equal(ComponentType.Button, component.Type);
		Assert.Equal("test", component.CustomId);
		Assert.Equal("Click Me", component.Label);
	}

	[Fact]
	public void Read_WithMultipleComponents_DeserializesAll()
	{
		// Arrange
		var json = """[{"type":1,"components":[]},{"type":2,"custom_id":"btn1"}]""";

		// Act
		var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(2, result.Length);
		Assert.IsType<ActionRowComponent>(result[0]);
		Assert.IsType<MessageComponent>(result[1]);
	}

	[Fact]
	public void Read_WithEmptyArray_ReturnsEmptyArray()
	{
		// Arrange
		var json = "[]";

		// Act
		var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public void Read_WithInvalidToken_ThrowsJsonException()
	{
		// Arrange
		var json = "{}";
		var bytes = System.Text.Encoding.UTF8.GetBytes(json);

		// Act & Assert
		var exception = Assert.Throws<JsonException>(() =>
		{
			var reader = new Utf8JsonReader(bytes);
			_converter.Read(ref reader, typeof(IInteractionComponent[]), _options);
		});
		Assert.Contains("Expected start of array", exception.Message);
	}

	[Fact]
	public void Read_WithMissingType_ThrowsJsonException()
	{
		// Arrange
		var json = """[{"custom_id":"test"}]""";

		// Act & Assert
		var exception = Assert.Throws<JsonException>(() =>
		{
			_converter.Read(json, typeof(IInteractionComponent[]), _options);
		});
		Assert.Contains("Component missing 'type' field", exception.Message);
	}

	[Fact]
	public void Read_WithSelectMenu_DeserializesMessageComponent()
	{
		// Arrange
		var json = """[{"type":3,"custom_id":"select1","placeholder":"Choose..."}]""";

		// Act
		var result = _converter.Read(json, typeof(IInteractionComponent[]), _options);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result);
		Assert.IsType<MessageComponent>(result[0]);
		var component = (MessageComponent)result[0];
		Assert.Equal(ComponentType.StringSelect, component.Type);
		Assert.Equal("select1", component.CustomId);
		Assert.Equal("Choose...", component.Placeholder);
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
	public void Write_WithActionRowComponent_SerializesCorrectly()
	{
		// Arrange
		var component = new ActionRowComponent
		{
			Type = ComponentType.ActionRow,
			Components = []
		};
		var components = new IInteractionComponent[] { component };
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, components, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("\"type\":1", json);
	}

	[Fact]
	public void Write_WithMessageComponent_SerializesCorrectly()
	{
		// Arrange
		var component = new MessageComponent
		{
			Type = ComponentType.Button,
			CustomId = "test_button",
			Label = "Click Me"
		};
		var components = new IInteractionComponent[] { component };
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, components, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("\"type\":2", json);
		Assert.Contains("test_button", json);
		Assert.Contains("Click Me", json);
	}

	[Fact]
	public void Write_WithMultipleComponents_SerializesAll()
	{
		// Arrange
		var actionRow = new ActionRowComponent
		{
			Type = ComponentType.ActionRow,
			Components = []
		};
		var button = new MessageComponent
		{
			Type = ComponentType.Button,
			CustomId = "btn1"
		};
		var components = new IInteractionComponent[] { actionRow, button };
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, components, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("\"type\":1", json);
		Assert.Contains("\"type\":2", json);
	}

	[Fact]
	public void Write_WithEmptyArray_SerializesEmptyArray()
	{
		// Arrange
		var components = Array.Empty<IInteractionComponent>();
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		// Act
		_converter.Write(writer, components, _options);
		writer.Flush();

		// Assert
		var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		Assert.Equal("[]", json);
	}
}

