using System.Globalization;

namespace DiscoSdk.Hosting.Rest;

internal static class RequestUtils
{

    internal static string? GetString(this HttpResponseMessage res, string name)
    {
        return res.Headers.TryGetValues(name, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    internal static int? GetInt(this HttpResponseMessage res, string name)
    {
        var value = GetString(res, name);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
            ? i
            : null;
    }

    internal static double? GetDouble(this HttpResponseMessage res, string name)
    {
        var value = GetString(res, name);
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
            ? d
            : null;
    }
}
