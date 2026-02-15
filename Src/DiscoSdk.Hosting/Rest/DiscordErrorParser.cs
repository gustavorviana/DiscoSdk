using DiscoSdk.Rest;
using System.Text.Json;

namespace DiscoSdk.Hosting.Rest;

internal static class DiscordErrorParser
{
    public static DiscordApiError? Parse(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string? message = null;
        int? code = null;
        JsonElement errorsElement;

        // Detect envelope vs map-only
        if (root.TryGetProperty("message", out var msgProp) &&
            root.TryGetProperty("code", out var codeProp))
        {
            message = msgProp.GetString();
            code = codeProp.GetInt32();
            errorsElement = root.GetProperty("errors");
        }
        else
        {
            errorsElement = root;
        }

        var validationErrors = new List<DiscordValidationError>();

        foreach (var property in errorsElement.EnumerateObject())
        {
            ParseNode(
                property.Value,
                property.Name,
                null,
                validationErrors,
                parentIsIndex: false);
        }

        return new DiscordApiError
        {
            Message = message,
            Code = code,
            ValidationErrors = validationErrors
        };
    }

    private static void ParseNode(
        JsonElement element,
        string currentName,
        int? currentIndex,
        List<DiscordValidationError> output,
        bool parentIsIndex)
    {
        // Case: {"9": ["Application command ids must be unique"]}
        if (element.ValueKind == JsonValueKind.Array)
        {
            var messages = new List<string>();

            foreach (var item in element.EnumerateArray())
                messages.Add(item.GetString() ?? string.Empty);

            var isIndexName = int.TryParse(currentName, out var idx);

            output.Add(new DiscordValidationError
            {
                Index = isIndexName ? idx : currentIndex,
                Name = isIndexName ? null : currentName,
                Messages = messages
            });

            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
            return;

        // If this property name is numeric, treat as index
        if (int.TryParse(currentName, out var parsedIndex))
        {
            currentIndex = parsedIndex;
        }

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Name == "_errors")
            {
                var fieldErrors = new List<DiscordFieldError>();

                foreach (var err in prop.Value.EnumerateArray())
                {
                    fieldErrors.Add(new DiscordFieldError
                    {
                        Code = err.GetProperty("code").GetString() ?? string.Empty,
                        Message = err.GetProperty("message").GetString() ?? string.Empty
                    });
                }

                output.Add(new DiscordValidationError
                {
                    Index = currentIndex,
                    Name = parentIsIndex ? null : currentName,
                    FieldErrors = fieldErrors
                });

                continue;
            }

            ParseNode(
                prop.Value,
                prop.Name,
                currentIndex,
                output,
                parentIsIndex: int.TryParse(currentName, out _));
        }
    }
}