using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Commands;

internal readonly struct AutocompleteName(string commandName, string optionName, string? subcommand = null, string? subcommandGroup = null) : IEquatable<AutocompleteName>
{
    public string Name { get; } = BuildName(commandName, optionName, subcommand, subcommandGroup);

    public static AutocompleteName FromContext(IAutocompleteContext context)
    {
        return new AutocompleteName(context.CommandName, context.FocusedOption.Name, context.Subcommand, context.SubcommandGroup);
    }

    private static string BuildName(string commandName, string optionName, string? subcommand, string? subcommandGroup)
    {
        if (subcommandGroup != null)
            return $"{commandName}::{subcommandGroup}::{subcommand}::{optionName}";
        if (subcommand != null)
            return $"{commandName}::{subcommand}::{optionName}";
        return $"{commandName}::{optionName}";
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