using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Tests.Models.Messages;

public class TextInputBuilderTests
{
	[Fact]
	public void Constructor_WithValidParameters_CreatesBuilder()
	{
		// Arrange
		var customId = "input_1";
		var label = "Nome";
		var style = TextInputStyle.Short;

		// Act
		var builder = new TextInputBuilder(customId, label, style);

		// Assert
		Assert.NotNull(builder);
		var component = builder.Build();
		Assert.Equal(customId, component.CustomId);
		Assert.Equal(label, component.Label);
		Assert.Equal(style, component.Style);
	}

	[Fact]
	public void Constructor_WithShortStyle_SetsStyleToShort()
	{
		// Arrange
		var customId = "input_1";
		var label = "Nome";

		// Act
		var builder = new TextInputBuilder(customId, label, TextInputStyle.Short);

		// Assert
		var component = builder.Build();
		Assert.Equal(TextInputStyle.Short, component.Style);
	}

	[Fact]
	public void Constructor_WithParagraphStyle_SetsStyleToParagraph()
	{
		// Arrange
		var customId = "input_1";
		var label = "Descrição";

		// Act
		var builder = new TextInputBuilder(customId, label, TextInputStyle.Paragraph);

		// Assert
		var component = builder.Build();
		Assert.Equal(TextInputStyle.Paragraph, component.Style);
	}

	[Fact]
	public void Constructor_WithNullCustomId_ThrowsArgumentException()
	{
		// Arrange
		var label = "Nome";
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder(null!, label, style));
		Assert.Equal("Custom ID cannot be null or empty. (Parameter 'customId')", exception.Message);
		Assert.Equal("customId", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithEmptyCustomId_ThrowsArgumentException()
	{
		// Arrange
		var label = "Nome";
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder(string.Empty, label, style));
		Assert.Equal("Custom ID cannot be null or empty. (Parameter 'customId')", exception.Message);
		Assert.Equal("customId", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithWhitespaceCustomId_ThrowsArgumentException()
	{
		// Arrange
		var label = "Nome";
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder("   ", label, style));
		Assert.Equal("Custom ID cannot be null or empty. (Parameter 'customId')", exception.Message);
		Assert.Equal("customId", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithCustomIdExceeding100Characters_ThrowsArgumentException()
	{
		// Arrange
		var customId = new string('a', 101);
		var label = "Nome";
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder(customId, label, style));
		Assert.Equal("Custom ID cannot exceed 100 characters. (Parameter 'customId')", exception.Message);
		Assert.Equal("customId", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithCustomIdExactly100Characters_CreatesBuilder()
	{
		// Arrange
		var customId = new string('a', 100);
		var label = "Nome";
		var style = TextInputStyle.Short;

		// Act
		var builder = new TextInputBuilder(customId, label, style);

		// Assert
		var component = builder.Build();
		Assert.Equal(customId, component.CustomId);
	}

	[Fact]
	public void Constructor_WithNullLabel_ThrowsArgumentException()
	{
		// Arrange
		var customId = "input_1";
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder(customId, null!, style));
		Assert.Equal("Label cannot be null or empty. (Parameter 'label')", exception.Message);
		Assert.Equal("label", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithEmptyLabel_ThrowsArgumentException()
	{
		// Arrange
		var customId = "input_1";
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder(customId, string.Empty, style));
		Assert.Equal("Label cannot be null or empty. (Parameter 'label')", exception.Message);
		Assert.Equal("label", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithWhitespaceLabel_ThrowsArgumentException()
	{
		// Arrange
		var customId = "input_1";
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder(customId, "   ", style));
		Assert.Equal("Label cannot be null or empty. (Parameter 'label')", exception.Message);
		Assert.Equal("label", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithLabelExceeding45Characters_ThrowsArgumentException()
	{
		// Arrange
		var customId = "input_1";
		var label = new string('a', 46);
		var style = TextInputStyle.Short;

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new TextInputBuilder(customId, label, style));
		Assert.Equal("Label cannot exceed 45 characters. (Parameter 'label')", exception.Message);
		Assert.Equal("label", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithLabelExactly45Characters_CreatesBuilder()
	{
		// Arrange
		var customId = "input_1";
		var label = new string('a', 45);
		var style = TextInputStyle.Short;

		// Act
		var builder = new TextInputBuilder(customId, label, style);

		// Assert
		var component = builder.Build();
		Assert.Equal(label, component.Label);
	}

	[Fact]
	public void WithPlaceholder_WithValidPlaceholder_SetsPlaceholder()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);
		var placeholder = "Digite seu nome";

		// Act
		var result = builder.WithPlaceholder(placeholder);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(placeholder, component.Placeholder);
	}

	[Fact]
	public void WithPlaceholder_WithNull_SetsPlaceholderToNull()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithPlaceholder(null!);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Null(component.Placeholder);
	}

	[Fact]
	public void WithPlaceholder_WithEmptyString_SetsPlaceholderToEmpty()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithPlaceholder(string.Empty);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(string.Empty, component.Placeholder);
	}

	[Fact]
	public void WithPlaceholder_WithPlaceholderExceeding100Characters_ThrowsArgumentException()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);
		var placeholder = new string('a', 101);

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => builder.WithPlaceholder(placeholder));
		Assert.Equal("Placeholder cannot exceed 100 characters. (Parameter 'placeholder')", exception.Message);
		Assert.Equal("placeholder", exception.ParamName);
	}

	[Fact]
	public void WithPlaceholder_WithPlaceholderExactly100Characters_SetsPlaceholder()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);
		var placeholder = new string('a', 100);

		// Act
		var result = builder.WithPlaceholder(placeholder);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(placeholder, component.Placeholder);
	}

	[Fact]
	public void WithRequired_WithTrue_SetsRequiredToTrue()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithRequired(true);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.True(component.Required);
	}

	[Fact]
	public void WithRequired_WithFalse_SetsRequiredToFalse()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithRequired(false);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.False(component.Required);
	}

	[Fact]
	public void WithMinLength_WithValidValue_SetsMinLength()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);
		var minLength = 5;

		// Act
		var result = builder.WithMinLength(minLength);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(minLength, component.MinLength);
	}

	[Fact]
	public void WithMinLength_WithZero_SetsMinLengthToZero()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithMinLength(0);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(0, component.MinLength);
	}

	[Fact]
	public void WithMinLength_With4000_SetsMinLengthTo4000()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithMinLength(4000);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(4000, component.MinLength);
	}

	[Fact]
	public void WithMinLength_WithNegativeValue_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMinLength(-1));
		Assert.Equal("Min length must be between 0 and 4000. (Parameter 'minLength')", exception.Message);
		Assert.Equal("minLength", exception.ParamName);
	}

	[Fact]
	public void WithMinLength_WithValueGreaterThan4000_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMinLength(4001));
		Assert.Equal("Min length must be between 0 and 4000. (Parameter 'minLength')", exception.Message);
		Assert.Equal("minLength", exception.ParamName);
	}

	[Fact]
	public void WithMaxLength_WithValidValue_SetsMaxLength()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);
		var maxLength = 100;

		// Act
		var result = builder.WithMaxLength(maxLength);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(maxLength, component.MaxLength);
	}

	[Fact]
	public void WithMaxLength_WithOne_SetsMaxLengthToOne()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithMaxLength(1);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(1, component.MaxLength);
	}

	[Fact]
	public void WithMaxLength_With4000_SetsMaxLengthTo4000()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithMaxLength(4000);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(4000, component.MaxLength);
	}

	[Fact]
	public void WithMaxLength_WithZero_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMaxLength(0));
		Assert.Equal("Max length must be between 1 and 4000. (Parameter 'maxLength')", exception.Message);
		Assert.Equal("maxLength", exception.ParamName);
	}

	[Fact]
	public void WithMaxLength_WithValueGreaterThan4000_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act & Assert
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMaxLength(4001));
		Assert.Equal("Max length must be between 1 and 4000. (Parameter 'maxLength')", exception.Message);
		Assert.Equal("maxLength", exception.ParamName);
	}

	[Fact]
	public void WithValue_WithValidValue_SetsValue()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);
		var value = "Valor pré-preenchido";

		// Act
		var result = builder.WithValue(value);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(value, component.Value);
	}

	[Fact]
	public void WithValue_WithNull_SetsValueToNull()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithValue(null!);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Null(component.Value);
	}

	[Fact]
	public void WithValue_WithEmptyString_SetsValueToEmpty()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder.WithValue(string.Empty);

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal(string.Empty, component.Value);
	}

	[Fact]
	public void Build_WithValidBuilder_ReturnsTextInputComponent()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var component = builder.Build();

		// Assert
		Assert.NotNull(component);
		Assert.Equal("input_1", component.CustomId);
		Assert.Equal("Nome", component.Label);
		Assert.Equal(TextInputStyle.Short, component.Style);
	}

	[Fact]
	public void Build_WithMultipleCalls_ReturnsSameInstance()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var component1 = builder.Build();
		var component2 = builder.Build();

		// Assert
		Assert.Same(component1, component2);
	}

	[Fact]
	public void FluentInterface_AllowsMethodChaining()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		var result = builder
			.WithPlaceholder("Digite seu nome")
			.WithRequired(true)
			.WithMinLength(3)
			.WithMaxLength(50)
			.WithValue("Valor inicial");

		// Assert
		Assert.Same(builder, result);
		var component = builder.Build();
		Assert.Equal("Digite seu nome", component.Placeholder);
		Assert.True(component.Required);
		Assert.Equal(3, component.MinLength);
		Assert.Equal(50, component.MaxLength);
		Assert.Equal("Valor inicial", component.Value);
	}

	[Fact]
	public void ComplexTextInput_WithAllFeatures_BuildsCorrectly()
	{
		// Arrange
		var builder = new TextInputBuilder("description_input", "Descrição", TextInputStyle.Paragraph);

		// Act
		builder
			.WithPlaceholder("Descreva em detalhes...")
			.WithRequired(true)
			.WithMinLength(10)
			.WithMaxLength(500)
			.WithValue("Texto inicial");

		// Assert
		var component = builder.Build();
		Assert.Equal("description_input", component.CustomId);
		Assert.Equal("Descrição", component.Label);
		Assert.Equal(TextInputStyle.Paragraph, component.Style);
		Assert.Equal("Descreva em detalhes...", component.Placeholder);
		Assert.True(component.Required);
		Assert.Equal(10, component.MinLength);
		Assert.Equal(500, component.MaxLength);
		Assert.Equal("Texto inicial", component.Value);
	}

	[Fact]
	public void Build_WithMinAndMaxLength_ValidatesCorrectly()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		builder.WithMinLength(5).WithMaxLength(20);

		// Assert
		var component = builder.Build();
		Assert.Equal(5, component.MinLength);
		Assert.Equal(20, component.MaxLength);
	}

	[Fact]
	public void Build_WithMinLengthGreaterThanMaxLength_AllowsIt()
	{
		// Arrange
		var builder = new TextInputBuilder("input_1", "Nome", TextInputStyle.Short);

		// Act
		builder.WithMinLength(100).WithMaxLength(50);

		// Assert
		var component = builder.Build();
		Assert.Equal(100, component.MinLength);
		Assert.Equal(50, component.MaxLength);
		// Note: The builder doesn't validate this relationship, it's up to Discord API
	}
}

