namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class EnumChoicesAttribute<T>() : EnumChoicesAttribute(typeof(T)) where T : struct, Enum
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
            .Select(name => new ChoiceAttribute(name, name))];
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class EnumChoicesAttribute(Type enumType) : Attribute
{
    public string? OptionName { get; set; }

    public Type EnumType => enumType;

    internal virtual ChoiceAttribute[] GetChoices()
    {
        return [.. Enum.GetNames(enumType).Order().Select(x => new ChoiceAttribute(x, x))];
    }
}