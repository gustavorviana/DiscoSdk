using DiscoSdk.Hosting.Rest;

namespace DiscoSdk.Hosting.Tests.Rest;

public class DiscordErrorParserTests
{
    [Fact]
    public void Parse_WithNull_ReturnsNull()
    {
        // Arrange & Act
        var result = DiscordErrorParser.Parse(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithEmptyString_ReturnsNull()
    {
        // Arrange & Act
        var result = DiscordErrorParser.Parse(string.Empty);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Regression test for the previous bug where the parser invoked
    /// <c>root.GetProperty("errors")</c> without checking for presence.
    /// Discord routinely returns plain envelopes such as
    /// <c>{"message":"Missing Access","code":50001}</c> with no "errors" field,
    /// which used to throw <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public void Parse_EnvelopeWithoutErrorsField_ReturnsApiErrorWithMessageAndCode()
    {
        // Arrange
        var json = """{"message":"Missing Access","code":50001}""";

        // Act
        var result = DiscordErrorParser.Parse(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Missing Access", result.Message);
        Assert.Equal(50001, result.Code);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public void Parse_EnvelopeWithEmptyErrorsObject_ReturnsApiErrorWithoutValidationErrors()
    {
        // Arrange
        var json = """{"message":"Invalid Form Body","code":50035,"errors":{}}""";

        // Act
        var result = DiscordErrorParser.Parse(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Invalid Form Body", result.Message);
        Assert.Equal(50035, result.Code);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public void Parse_EnvelopeWithFieldErrors_FlattensFieldErrors()
    {
        // Arrange — Discord shape: {"errors":{"name":{"_errors":[{"code":"X","message":"Y"}]}}}
        var json = """
                   {
                     "message": "Invalid Form Body",
                     "code": 50035,
                     "errors": {
                       "name": {
                         "_errors": [
                           { "code": "BASE_TYPE_REQUIRED", "message": "This field is required" }
                         ]
                       }
                     }
                   }
                   """;

        // Act
        var result = DiscordErrorParser.Parse(json);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        var validation = result.ValidationErrors[0];
        Assert.Equal("name", validation.Name);
        Assert.Null(validation.Index);
        Assert.Single(validation.FieldErrors);
        Assert.Equal("BASE_TYPE_REQUIRED", validation.FieldErrors[0].Code);
        Assert.Equal("This field is required", validation.FieldErrors[0].Message);
    }

    [Fact]
    public void Parse_MapOnlyPayloadWithIndexedStringMessages_PopulatesIndexAndMessages()
    {
        // Arrange — Discord shape: {"9":["Application command ids must be unique"]}
        // The whole root is the errors map (no envelope).
        var json = """{"9":["Application command ids must be unique"]}""";

        // Act
        var result = DiscordErrorParser.Parse(json);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Message);
        Assert.Null(result.Code);
        Assert.Single(result.ValidationErrors);

        var validation = result.ValidationErrors[0];
        Assert.Equal(9, validation.Index);
        Assert.Null(validation.Name);
        Assert.Single(validation.Messages);
        Assert.Equal("Application command ids must be unique", validation.Messages[0]);
    }

    [Fact]
    public void Parse_EnvelopeWithIndexedFieldErrors_PopulatesIndexAndFieldErrors()
    {
        // Arrange — bulk overwrite shape: {"errors":{"0":{"name":{"_errors":[...]}}}}
        var json = """
                   {
                     "message": "Invalid Form Body",
                     "code": 50035,
                     "errors": {
                       "0": {
                         "name": {
                           "_errors": [
                             { "code": "STRING_TYPE_REGEX", "message": "Must match pattern" }
                           ]
                         }
                       }
                     }
                   }
                   """;

        // Act
        var result = DiscordErrorParser.Parse(json);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.ValidationErrors);

        var validation = result.ValidationErrors[0];
        Assert.Equal(0, validation.Index);
        Assert.Equal("name", validation.Name);
        Assert.Single(validation.FieldErrors);
        Assert.Equal("STRING_TYPE_REGEX", validation.FieldErrors[0].Code);
    }

    [Fact]
    public void Parse_WithMessageButNoCode_FallsBackToMapOnlyShape()
    {
        // Arrange — only "message" present, no "code". Treated as map-only and "message" is iterated.
        var json = """{"message":"x"}""";

        // Act
        var result = DiscordErrorParser.Parse(json);

        // Assert — the "message" field doesn't match an array/object pattern, so no validation entries.
        // The point of this test is that parsing does not throw on this shape.
        Assert.NotNull(result);
        Assert.Null(result.Message);
        Assert.Null(result.Code);
    }
}
