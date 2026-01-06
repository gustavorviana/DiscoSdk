using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

internal static class JsonUtils
{
    public static bool? TryGetBoolean(this JsonElement element)
    {
        var type = element.ValueKind;
        if (type == JsonValueKind.Number)
            return element.GetInt32() == 1;

        return
            type == JsonValueKind.True;
    }
}
