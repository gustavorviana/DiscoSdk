using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;
using System.Collections.Immutable;

namespace DiscoSdk;

/// <summary>
/// Represents a Discord entity that can contain permission overwrites.
/// </summary>
public interface IPermissionContainer
{
	/// <summary>
	/// Gets the permission overwrites for this container.
	/// </summary>
	PermissionOverwrite[]? PermissionOverwrites { get; }

	/// <summary>
	/// Gets the permission override for the specified permission holder, if it exists.
	/// </summary>
	/// <param name="holder">The permission holder (user or role) to get the override for.</param>
	/// <returns>The permission override, or null if it doesn't exist.</returns>
	PermissionOverride? GetPermissionOverride(IPermissionHolder holder);

	/// <summary>
	/// Gets all permission overrides for this container.
	/// </summary>
	/// <returns>An immutable list of all permission overrides.</returns>
	ImmutableList<PermissionOverride> GetPermissionOverrides();

	/// <summary>
	/// Gets all member permission overrides for this container.
	/// </summary>
	/// <returns>An immutable list of member permission overrides.</returns>
	ImmutableList<PermissionOverride> GetMemberPermissionOverrides()
	{
		return [.. GetPermissionOverrides().Where(po => po.IsMemberOverride)];
	}

	/// <summary>
	/// Gets all role permission overrides for this container.
	/// </summary>
	/// <returns>An immutable list of role permission overrides.</returns>
	ImmutableList<PermissionOverride> GetRolePermissionOverrides()
	{
		return [.. GetPermissionOverrides().Where(po => po.IsRoleOverride)];
	}

	/// <summary>
	/// Creates or updates a permission override for the specified permission holder.
	/// </summary>
	/// <param name="holder">The permission holder (user or role) to create or update the override for.</param>
	/// <returns>A REST action that can be executed to upsert the permission override.</returns>
	OverrideIPermissionAction UpsertPermissionOverride(IPermissionHolder holder);
}