using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Commands;

internal readonly struct AutocompleteName(string commandName, string optionName) : IEquatable<AutocompleteName>
{
    public string Name { get; } = $"{commandName}::{optionName}";

    public static AutocompleteName FromContext(IAutocompleteContext context)
    {
        return new AutocompleteName(context.CommandName, context.FocusedOption.Name);
    }

    public override bool Equals(object? obj)
    {
        return obj is AutocompleteName name && Equals(name);
    }

    public bool Equals(AutocompleteName other)
    {
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name);
    }

    public static bool operator ==(AutocompleteName left, AutocompleteName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AutocompleteName left, AutocompleteName right)
    {
        return !(left == right);
    }
}