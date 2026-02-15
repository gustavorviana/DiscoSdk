namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EnumChoicesAttribute(string optionName, Type enumType) : Attribute
{
    public string OptionName => optionName;
    public Type EnumType => enumType;

    internal ChoiceAttribute[] GetChoices()
    {
        return [.. Enum.GetNames(enumType).Order().Select(x => new ChoiceAttribute(optionName, x, x))];
    }
}
