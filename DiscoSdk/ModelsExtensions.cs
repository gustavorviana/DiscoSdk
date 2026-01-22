using DiscoSdk.Models.Enums;

namespace DiscoSdk;

/// <summary>
/// Extension methods for <see cref="IDeletable"/>.
/// </summary>
public static class ModelsExtensions
{
	public static bool HasPermission(this IPermissible permissible, DiscordPermission permission)
	{
		if (permissible.Permissions == DiscordPermission.Administrator)
			return true;

		return permission != DiscordPermission.None &&  permissible.Permissions.HasFlag(permission);
    }
}