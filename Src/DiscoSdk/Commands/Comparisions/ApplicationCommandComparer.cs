using DiscoSdk.Models.Commands;
using System.Diagnostics.CodeAnalysis;

namespace DiscoSdk.Commands.Comparisions;

internal class ApplicationCommandComparer : EqualityComparer<ApplicationCommand>
{
    public override bool Equals(ApplicationCommand? x, ApplicationCommand? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode([DisallowNull] ApplicationCommand obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
    }
}