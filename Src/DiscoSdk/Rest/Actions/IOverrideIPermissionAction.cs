using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for upserting (insert or update) a permission override.
/// </summary>
public interface IOverrideIPermissionAction : IRestAction<PermissionOverride>
{
	/// <summary>
	/// Sets the allowed permissions.
	/// </summary>
	/// <param name="permissions">The permissions to allow.</param>
	/// <returns>The current <see cref="IOverrideIPermissionAction"/> instance.</returns>
	IOverrideIPermissionAction SetAllow(DiscordPermission permissions);

	/// <summary>
	/// Sets the denied permissions.
	/// </summary>
	/// <param name="permissions">The permissions to deny.</param>
	/// <returns>The current <see cref="IOverrideIPermissionAction"/> instance.</returns>
	IOverrideIPermissionAction SetDeny(DiscordPermission permissions);
}

