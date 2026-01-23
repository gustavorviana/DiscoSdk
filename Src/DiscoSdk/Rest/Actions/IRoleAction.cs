using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for creating or updating a role.
/// </summary>
public interface IRoleAction : IRestAction<IRole>
{
	/// <summary>
	/// Sets the name of the role.
	/// </summary>
	/// <param name="name">The role name.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetName(string name);

	/// <summary>
	/// Sets the permissions of the role.
	/// </summary>
	/// <param name="permissions">The permissions to set.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetPermissions(DiscordPermission permissions);

	/// <summary>
	/// Sets the color of the role.
	/// </summary>
	/// <param name="color">The color to set.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetColor(Color color);

	/// <summary>
	/// Sets whether the role should be hoisted.
	/// </summary>
	/// <param name="hoist">True to hoist the role, false otherwise.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetHoist(bool hoist);

	/// <summary>
	/// Sets whether the role should be mentionable.
	/// </summary>
	/// <param name="mentionable">True to make the role mentionable, false otherwise.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetMentionable(bool mentionable);

	/// <summary>
	/// Sets the position of the role in the role hierarchy.
	/// </summary>
	/// <param name="position">The position to set.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetPosition(int? position);

	/// <summary>
	/// Sets the icon of the role from image data.
	/// </summary>
	/// <param name="icon">The icon image data, or null to remove it.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetIcon(byte[]? icon);

	/// <summary>
	/// Sets the icon of the role from a hash.
	/// </summary>
	/// <param name="iconHash">The icon hash, or null to remove it.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetIcon(string? iconHash);

	/// <summary>
	/// Sets the unicode emoji icon of the role.
	/// </summary>
	/// <param name="unicodeEmoji">The unicode emoji, or null to remove it.</param>
	/// <returns>The current <see cref="IRoleAction"/> instance.</returns>
	IRoleAction SetUnicodeEmoji(string? unicodeEmoji);
}