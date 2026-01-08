using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord guild category channel.
/// </summary>
public interface IEditGuildCategoryChannelRestAction : IRestAction<IGuildCategoryChannel>
{
	/// <summary>
	/// Sets the name of the channel.
	/// </summary>
	/// <param name="name">The new name of the channel (1-100 characters).</param>
	/// <returns>The current <see cref="IEditGuildCategoryChannelRestAction"/> instance.</returns>
	IEditGuildCategoryChannelRestAction SetName(string? name);

	/// <summary>
	/// Sets the position of the channel.
	/// </summary>
	/// <param name="position">The new position of the channel.</param>
	/// <returns>The current <see cref="IEditGuildCategoryChannelRestAction"/> instance.</returns>
	IEditGuildCategoryChannelRestAction SetPosition(int? position);

	/// <summary>
	/// Sets the permission overwrites for the channel.
	/// </summary>
	/// <param name="permissionOverwrites">The permission overwrites.</param>
	/// <returns>The current <see cref="IEditGuildCategoryChannelRestAction"/> instance.</returns>
	IEditGuildCategoryChannelRestAction SetPermissionOverwrites(PermissionOverwrite[]? permissionOverwrites);

	/// <summary>
	/// Sets the channel flags.
	/// </summary>
	/// <param name="flags">The channel flags.</param>
	/// <returns>The current <see cref="IEditGuildCategoryChannelRestAction"/> instance.</returns>
	IEditGuildCategoryChannelRestAction SetFlags(ChannelFlags? flags);
}

