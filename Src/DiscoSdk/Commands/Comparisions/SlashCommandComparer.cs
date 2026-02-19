using DiscoSdk.Models.Commands;
using System.Diagnostics.CodeAnalysis;

namespace DiscoSdk.Commands.Comparisions;

internal class SlashCommandComparer : EqualityComparer<SlashCommand>
{
    public override bool Equals(SlashCommand? x, SlashCommand? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode([DisallowNull] SlashCommand obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
    }
}