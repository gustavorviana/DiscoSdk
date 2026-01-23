using DiscoSdk.Models.Enums;

namespace DiscoSdk;

public interface IPermissible
{
    /// <summary>
    /// Gets the computed permissions for the invoking user in the channel.
    /// </summary>
    DiscordPermission Permissions { get; }
}
