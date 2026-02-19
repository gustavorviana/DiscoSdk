namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class AutocompleteHandlerAttribute : Attribute
{
    public string OptionName { get; }
    public string CommandName { get; }

    public AutocompleteHandlerAttribute(string commandName, string optionName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Command name cannot be null, empty, or whitespace.", nameof(commandName));

        if (string.IsNullOrWhiteSpace(optionName))
            throw new ArgumentException("Option name cannot be null, empty, or whitespace.", nameof(optionName));

        CommandName = commandName;
        OptionName = optionName;
    }
}
