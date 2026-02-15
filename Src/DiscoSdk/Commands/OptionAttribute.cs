using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class OptionAttribute(
    ApplicationCommandOptionType type,
    string name,
    string description,
    bool required = false) : Attribute
{
    private int? minLength = null;
    private int? maxLength = null;

    public ApplicationCommandOptionType Type => type;
    public string Name => name;
    public string Description => description;
    public bool Required => required;

    public int MinLength
    {
        get => minLength ?? -1;
        set => minLength = value;
    }

    public int MaxLength
    {
        get => maxLength ?? -1;
        set => maxLength = value;
    }

    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }

    public Type? AutocompleteType { get; set; }

    public ChannelType[]? ChannelTypes { get; set; }

    public int? GetMinLength() => minLength;
    public int? GetMaxLength() => maxLength;
}