using DiscoSdk.Models.Commands;

namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ChoiceAttribute(string optionName, string name, object value) : Attribute
{
    public string OptionName => optionName;
    public string Name => name;
    public object Value => value;

    public ApplicationCommandOptionChoice ToCommandChoice()
    {
        return new ApplicationCommandOptionChoice
        {
            Name = Name,
            Value = Value
        };
    }
}