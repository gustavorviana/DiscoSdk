using DiscoSdk.Models.Commands;
using System.Diagnostics.CodeAnalysis;

namespace DiscoSdk.Commands.Comparisions;

internal class SlashCommandComparer : EqualityComparer<ApplicationCommand>
{
    public override bool Equals(ApplicationCommand? x, ApplicationCommand? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
            && (x.Type ?? Models.Enums.ApplicationCommandType.ChatInput) == (y.Type ?? Models.Enums.ApplicationCommandType.ChatInput);
    }

    public override int GetHashCode([DisallowNull] ApplicationCommand obj)
        => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name),
            obj.Type ?? Models.Enums.ApplicationCommandType.ChatInput);
}