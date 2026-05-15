using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Commands;

/// <summary>
/// Centralizes the <see cref="ApplicationCommandType"/> validations that the registry and its
/// builder need at multiple call sites: rejecting unknown enum values on lookup paths and
/// asserting "must be a context menu" on the context-menu add path.
/// </summary>
internal static class ApplicationCommandTypeGuard
{
    /// <summary>
    /// Returns an exception for an <see cref="ApplicationCommandType"/> that isn't one of
    /// the three supported values (ChatInput / User / Message). Caller throws.
    /// </summary>
    public static ArgumentOutOfRangeException Unsupported(ApplicationCommandType type, string paramName)
        => new(paramName, $"Unsupported ApplicationCommandType: {type}.");

    /// <summary>
    /// Throws if <paramref name="type"/> isn't <see cref="ApplicationCommandType.User"/> or
    /// <see cref="ApplicationCommandType.Message"/>. Used by the context-menu add path —
    /// <see cref="ApplicationCommandType.ChatInput"/> is rejected here even though it's a
    /// valid lookup type elsewhere.
    /// </summary>
    public static void EnsureContextMenu(ApplicationCommandType type, string paramName)
    {
        if (type is not ApplicationCommandType.User and not ApplicationCommandType.Message)
            throw new ArgumentOutOfRangeException(paramName,
                $"Context menu type must be User or Message, got {type}.");
    }
}
