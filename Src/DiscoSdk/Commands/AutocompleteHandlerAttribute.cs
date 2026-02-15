namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class AutocompleteHandlerAttribute(string optionName) : Attribute
{
    public string OptionName => optionName;
}
