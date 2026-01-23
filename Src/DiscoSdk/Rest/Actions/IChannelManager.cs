using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for channel operations with type-safe method chaining.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class, used for method chaining.</typeparam>
public interface IChannelManager<TSelf> : IManager<TSelf> where TSelf : IChannelManager<TSelf>
{
	/// <summary>
	/// Sets the name of the channel.
	/// </summary>
	/// <param name="name">The new name for the channel.</param>
	/// <returns>The current <see cref="IChannelManager{TSelf}"/> instance.</returns>
	TSelf SetName(string name);

	/// <summary>
	/// Sets the parent category of the channel.
	/// </summary>
	/// <param name="parentId">The ID of the parent category, or null to remove from category.</param>
	/// <returns>The current <see cref="IChannelManager{TSelf}"/> instance.</returns>
	TSelf SetParent(Snowflake? parentId);

	/// <summary>
	/// Sets the position of the channel.
	/// </summary>
	/// <param name="position">The new position for the channel.</param>
	/// <returns>The current <see cref="IChannelManager{TSelf}"/> instance.</returns>
	TSelf SetPosition(int position);

	/// <summary>
	/// Adds or updates a permission override for the specified target.
	/// </summary>
	/// <param name="targetId">The ID of the role or member.</param>
	/// <param name="isRole">True if the target is a role, false if it's a member.</param>
	/// <param name="allow">The permissions to allow.</param>
	/// <param name="deny">The permissions to deny.</param>
	/// <returns>The current <see cref="IChannelManager{TSelf}"/> instance.</returns>
	TSelf PutPermissionOverride(ulong targetId, bool isRole, DiscordPermission allow, DiscordPermission deny);

	/// <summary>
	/// Removes a permission override for the specified target.
	/// </summary>
	/// <param name="targetId">The ID of the role or member.</param>
	/// <param name="isRole">True if the target is a role, false if it's a member.</param>
	/// <returns>The current <see cref="IChannelManager{TSelf}"/> instance.</returns>
	TSelf RemovePermissionOverride(ulong targetId, bool isRole);

	/// <summary>
	/// Synchronizes permissions with the parent category.
	/// </summary>
	/// <returns>The current <see cref="IChannelManager{TSelf}"/> instance.</returns>
	TSelf SyncPermissions();
}

