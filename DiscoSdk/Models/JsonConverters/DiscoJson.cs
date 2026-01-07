using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

public static class DiscoJson
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

        return options;
    }
}
