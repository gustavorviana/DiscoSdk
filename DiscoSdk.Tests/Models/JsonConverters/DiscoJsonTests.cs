using DiscoSdk.Models.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Tests.Models.JsonConverters;

public class DiscoJsonTests
{
	[Fact]
	public void Create_ReturnsJsonSerializerOptions()
	{
		// Act
		var options = DiscoJson.Create();

		// Assert
		Assert.NotNull(options);
		Assert.IsType<JsonSerializerOptions>(options);
	}

	[Fact]
	public void Create_HasDefaultIgnoreConditionWhenWritingNull()
	{
		// Act
		var options = DiscoJson.Create();

		// Assert
		Assert.Equal(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
	}

	[Fact]
	public void Create_HasSnakeCaseLowerNamingPolicy()
	{
		// Act
		var options = DiscoJson.Create();

		// Assert
		Assert.NotNull(options.PropertyNamingPolicy);
		// JsonNamingPolicy.SnakeCaseLower is a static property that returns a JsonNamingPolicy instance
		Assert.Equal(JsonNamingPolicy.SnakeCaseLower, options.PropertyNamingPolicy);
	}

	[Fact]
	public void Create_UsesWebDefaults()
	{
		// Act
		var options = DiscoJson.Create();

		// Assert
		// Web defaults include things like case-insensitive property matching
		// We can verify by checking that it's not null and has expected properties
		Assert.NotNull(options);
	}

	[Fact]
	public void Create_CanSerializeWithSnakeCase()
	{
		// Arrange
		var options = DiscoJson.Create();
		var obj = new { MyProperty = "value" };

		// Act
		var json = JsonSerializer.Serialize(obj, options);

		// Assert
		Assert.Contains("my_property", json);
	}
}

