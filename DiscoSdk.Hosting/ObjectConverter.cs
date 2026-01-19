using DiscoSdk.Models;
using System.Globalization;

namespace DiscoSdk.Hosting;

public interface IObjectConverter
{
    object? Convert(Type targetType, object? value);
}

public class ObjectConverter(CultureInfo cultureInfo) : IObjectConverter
{
    public object? Convert(Type targetType, object? value)
    {
        if (value == null)
            return null;

        var nonNullable = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (nonNullable.IsEnum)
            return SafeParseEnum(nonNullable, value);

        if (typeof(IConvertible).IsAssignableFrom(nonNullable))
            return System.Convert.ChangeType(value, nonNullable, cultureInfo);

        if (value is IConvertible convertible)
            return convertible.ToType(targetType, cultureInfo);

        if (value is string strValue && targetType == typeof(Snowflake))
            return Snowflake.Parse(strValue);

        if (value is ulong ulongValue && targetType == typeof(Snowflake))
            return new Snowflake(ulongValue);

        if (value is long longValue && targetType == typeof(Snowflake))
            return new Snowflake((ulong)longValue);

        throw new NotSupportedException(
        $"Conversion not supported: valueType={value.GetType().FullName} targetType={targetType.FullName}");
    }

    public static object? SafeParseEnum(Type enumType, object value)
    {
        if (enumType.IsInstanceOfType(value))
            return value;

        try
        {
            var underlying = Enum.GetUnderlyingType(enumType);

            // Numeric value (int, long, etc.)
            if (value is IConvertible)
            {
                var numeric = System.Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);

                if (Enum.IsDefined(enumType, numeric))
                    return Enum.ToObject(enumType, numeric);
            }
        }
        catch
        {
        }

        if (Enum.TryParse(enumType, value.ToString(), ignoreCase: true, out var result))
            return result;

        return null;
    }
}