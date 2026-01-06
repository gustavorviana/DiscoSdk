using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Utility methods for JSON element operations.
/// </summary>
internal static class JsonUtils
{
    /// <summary>
    /// Attempts to get a boolean value from a JSON element, handling both boolean and numeric (0/1) representations.
    /// </summary>
    /// <param name="element">The JSON element to extract the boolean value from.</param>
    /// <returns>The boolean value if the element represents a boolean or numeric 1; null if the element is not a boolean or numeric value.</returns>
    public static bool? TryGetBoolean(this JsonElement element)
    {
        var type = element.ValueKind;
        if (type == JsonValueKind.Number)
            return element.GetInt32() == 1;

        return
            type == JsonValueKind.True;
    }
}
