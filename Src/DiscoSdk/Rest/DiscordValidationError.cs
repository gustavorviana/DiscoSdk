namespace DiscoSdk.Rest;

public sealed class DiscordValidationError
{
    /// <summary>
    /// Optional index when the error comes from a list item (e.g. bulk overwrite).
    /// Example: options[0].name => Index = 0, Name = "name"
    /// Example: {"9": ["..."]} => Index = 9, Name = null
    /// </summary>
    public int? Index { get; init; }

    /// <summary>
    /// The logical field name the error refers to (e.g. "name", "description", "options").
    /// Null when the error is for the whole indexed item (e.g. "Application command ids must be unique").
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Discord "_errors" items (code + message).
    /// </summary>
    public IReadOnlyList<DiscordFieldError> FieldErrors { get; init; } = [];

    /// <summary>
    /// Plain string messages (e.g. {"9": ["Application command ids must be unique"]}).
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } = [];

    /// <summary>
    /// Optional fully-qualified normalized path for debugging/logging.
    /// Example: "options[0].name" or "[9]".
    /// </summary>
    public string? Path { get; init; }
}