namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EnumChoicesAttribute<T>(string optionName) : EnumChoicesAttribute(optionName, typeof(T)) where T : struct, Enum
{
    public T[] ExceptValues = [];

    internal override ChoiceAttribute[] GetChoices()
    {
        var except = ExceptValues ?? [];
        if (except.Length == 0)
            return base.GetChoices();

        return [.. Enum.GetValues<T>()
            .Where(v => Array.IndexOf(except, v) < 0)
            .Select(v => v.ToString())
            .Order()
            .Select(name => new ChoiceAttribute(OptionName, name, name))];
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class EnumChoicesAttribute(string optionName, Type enumType) : Attribute
{
    public string OptionName => optionName;
    public Type EnumType => enumType;

    internal virtual ChoiceAttribute[] GetChoices()
    {
        return [.. Enum.GetNames(enumType).Order().Select(x => new ChoiceAttribute(optionName, x, x))];
    }
}