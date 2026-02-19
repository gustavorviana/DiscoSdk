using DiscoSdk.Commands;
using DiscoSdk.Models.Enums;
using System.Reflection;

namespace DiscoSdk.Utils;

internal static class SlashOptionTypeUtils
{
    public static ChoiceAttribute[] GetChoices(MethodInfo method, string optionName)
    {
        var choices = new List<ChoiceAttribute>();

        var enumChoices = method.GetCustomAttributes<EnumChoicesAttribute>()
            .FirstOrDefault(a => string.Equals(a.OptionName, optionName, StringComparison.OrdinalIgnoreCase));

        if (enumChoices != null)
            choices.AddRange(enumChoices.GetChoices());

        choices.AddRange(method.GetCustomAttributes<ChoiceAttribute>()
            .Where(c => string.Equals(c.OptionName, optionName, StringComparison.OrdinalIgnoreCase)));

        return [.. choices];
    }

    public static ChoiceAttribute[] GetChoices(ParameterInfo parameter)
    {
        var choices = new List<ChoiceAttribute>();

        var enumChoices = parameter.ParameterType.GetCustomAttribute<EnumChoicesAttribute>();
        if (enumChoices != null)
            choices.AddRange(enumChoices.GetChoices());

        choices.AddRange(parameter.ParameterType.GetCustomAttributes<ChoiceAttribute>());
        choices.AddRange(parameter.GetCustomAttributes<ChoiceAttribute>());

        if (choices.Count == 0)
            choices.AddRange(Enum.GetNames(parameter.ParameterType).Order().Select(x => new ChoiceAttribute(x, x)));

        return [.. choices];
    }


    private const long DiscordIntegerMax = 9_007_199_254_740_991L; // 2^53 - 1
    private const long DiscordIntegerMin = -9_007_199_254_740_991L; // -(2^53 - 1)

    public static Dictionary<Type, SlashCommandOptionType> GetCommandTypeAssociations()
    {
        Dictionary<Type, SlashCommandOptionType> _typeMaps = [];

        _typeMaps[typeof(string)] = SlashCommandOptionType.String;

        _typeMaps[typeof(bool)] = SlashCommandOptionType.Boolean;

        _typeMaps[typeof(sbyte)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(byte)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(short)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(ushort)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(int)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(uint)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(long)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(ulong)] = SlashCommandOptionType.Integer;

        _typeMaps[typeof(float)] = SlashCommandOptionType.Number;
        _typeMaps[typeof(double)] = SlashCommandOptionType.Number;

        // Optional common mappings
        _typeMaps[typeof(char)] = SlashCommandOptionType.String;

        // Nullable variants
        _typeMaps[typeof(bool?)] = SlashCommandOptionType.Boolean;

        _typeMaps[typeof(sbyte?)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(byte?)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(short?)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(ushort?)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(int?)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(uint?)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(long?)] = SlashCommandOptionType.Integer;
        _typeMaps[typeof(ulong?)] = SlashCommandOptionType.Integer;

        _typeMaps[typeof(float?)] = SlashCommandOptionType.Number;
        _typeMaps[typeof(double?)] = SlashCommandOptionType.Number;

        _typeMaps[typeof(char?)] = SlashCommandOptionType.String;

        return _typeMaps;
    }

    public static bool IsRequired(Type type)
    {
        return type.IsValueType && Nullable.GetUnderlyingType(type) == null; // struct (incl. primitives, enums, DateTime, etc.) => required
    }

    public static int? GetMaxLength(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(string))
            return null;

        if (underlying == typeof(char))
            return 1;

        return null;
    }

    public static long? GetMinValue(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(byte) ||
            underlying == typeof(ushort) ||
            underlying == typeof(uint) ||
            underlying == typeof(ulong))
            return 0;

        if (underlying == typeof(sbyte)) return sbyte.MinValue;
        if (underlying == typeof(short)) return short.MinValue;
        if (underlying == typeof(int)) return int.MinValue;

        if (underlying == typeof(long)) return DiscordIntegerMin;

        return null;
    }

    public static long? GetMaxValue(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(byte)) return byte.MaxValue;
        if (underlying == typeof(ushort)) return ushort.MaxValue;
        if (underlying == typeof(uint)) return uint.MaxValue;

        if (underlying == typeof(ulong)) return DiscordIntegerMax;

        if (underlying == typeof(sbyte)) return sbyte.MaxValue;
        if (underlying == typeof(short)) return short.MaxValue;
        if (underlying == typeof(int)) return int.MaxValue;

        if (underlying == typeof(long)) return DiscordIntegerMax;

        return null;
    }
}
