using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for upserting (insert or update) a permission override.
/// </summary>
public interface OverrideIPermissionAction : IRestAction<PermissionOverride>
{
	/// <summary>
	/// Sets the allowed permissions.
	/// </summary>
	/// <param name="permissions">The permissions to allow.</param>
	/// <returns>The current <see cref="OverrideIPermissionAction"/> instance.</returns>
	OverrideIPermissionAction SetAllow(DiscordPermission permissions);

	/// <summary>
	/// Sets the denied permissions.
	/// </summary>
	/// <param name="permissions">The permissions to deny.</param>
	/// <returns>The current <see cref="OverrideIPermissionAction"/> instance.</returns>
	OverrideIPermissionAction SetDeny(DiscordPermission permissions);
}

