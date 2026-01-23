using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord role.
/// </summary>
public interface IRole : IMentionable, IPermissionHolder, IComparable<IRole>, IDeletable
{
	/// <summary>
	/// The default color raw value (536870911).
	/// </summary>
	const int DefaultColorRaw = 536870911;

	/// <summary>
	/// Gets the position of this role in the role hierarchy.
	/// </summary>
	int Position { get; }

	/// <summary>
	/// Gets the raw position value of this role.
	/// </summary>
	int PositionRaw { get; }

	/// <summary>
	/// Gets the name of this role.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets a value indicating whether this role is managed by an integration.
	/// </summary>
	bool IsManaged { get; }

	/// <summary>
	/// Gets a value indicating whether this role is hoisted (displayed separately in the member list).
	/// </summary>
	bool IsHoisted { get; }

	/// <summary>
	/// Gets a value indicating whether this role is mentionable.
	/// </summary>
	bool IsMentionable { get; }

	/// <summary>
	/// Gets the raw permissions value of this role.
	/// </summary>
	DiscordPermission Permissions { get; }

    /// <summary>
    /// Gets the colors of this role.
    /// </summary>
    Color Colors { get; }

	/// <summary>
	/// Gets a value indicating whether this is the public role (@everyone).
	/// </summary>
	bool IsPublicRole { get; }

    /// <summary>
    /// Gets the guild this role belongs to.
    /// </summary>
    IGuild Guild { get; }

	/// <summary>
	/// Gets the tags associated with this role.
	/// </summary>
	RoleTags Tags { get; }

	/// <summary>
	/// Gets the icon of this role, if any.
	/// </summary>
	RoleIcon? Icon { get; }

	/// <summary>
	/// Determines whether this role can interact with the specified role.
	/// </summary>
	/// <param name="role">The role to check interaction with.</param>
	/// <returns>True if this role can interact with the specified role, false otherwise.</returns>
	bool CanInteract(IRole role);

	/// <summary>
	/// Gets a REST action for editing this role.
	/// </summary>
	/// <returns>A REST action that can be configured and executed to edit the role.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRoleAction Edit();

	/// <summary>
	/// Gets a REST action for modifying the position of this role.
	/// </summary>
	/// <param name="position">The new position for the role.</param>
	/// <returns>A REST action that can be executed to modify the role position.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction ModifyPosition(int position);

	/// <summary>
	/// Creates a copy of this role in the specified guild.
	/// </summary>
	/// <param name="guild">The guild to create the copy in.</param>
	/// <returns>A REST action that can be executed to create the role copy.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRoleAction CreateCopy(IGuild guild);

	/// <summary>
	/// Creates a copy of this role in the same guild.
	/// </summary>
	/// <returns>A REST action that can be executed to create the role copy.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRoleAction CreateCopy()
	{
		return CreateCopy(Guild);
	}
}