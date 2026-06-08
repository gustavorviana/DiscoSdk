using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Commands;

internal readonly struct AutoCompleteName(string commandName, string optionName, string? subcommand = null, string? subcommandGroup = null) : IEquatable<AutoCompleteName>
{
    public string Name { get; } = BuildName(commandName, optionName, subcommand, subcommandGroup);

    public static AutoCompleteName FromContext(IAutoCompleteContext context)
    {
        return new AutoCompleteName(context.CommandName, context.FocusedOption.Name, context.Subcommand, context.SubcommandGroup);
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
        return obj is AutoCompleteName name && Equals(name);
    }

    public bool Equals(AutoCompleteName other)
    {
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }

    public static bool operator ==(AutoCompleteName left, AutoCompleteName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AutoCompleteName left, AutoCompleteName right)
    {
        return !(left == right);
    }
}