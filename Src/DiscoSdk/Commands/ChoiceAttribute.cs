using DiscoSdk.Models.Commands;

namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class ChoiceAttribute(string name, object value) : Attribute
{
    public string? OptionName { get; set; }
    public string Name => name;
    public object Value => value;

    public SlashCommandOptionChoice ToCommandChoice()
    {
        return new SlashCommandOptionChoice
        {
            Name = Name,
            Value = Value,
        };
    }
}