using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord channel that belongs to a guild.
/// </summary>
public interface IGuildChannel : IChannel, IDeletable
{
	/// <summary>
	/// Gets the ID of the guild this channel belongs to.
	/// </summary>
	DiscordId GuildId { get; }

	/// <summary>
	/// Gets the sorting position of the channel.
	/// </summary>
	int? Position { get; }

	/// <summary>
	/// Gets the explicit permission overwrites for members and roles.
	/// </summary>
	PermissionOverwrite[]? PermissionOverwrites { get; }

	/// <summary>
	/// Gets the name of the channel.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets a value indicating whether the channel is NSFW.
	/// </summary>
	bool? Nsfw { get; }

	/// <summary>
	/// Gets the ID of the parent channel for this channel.
	/// </summary>
	DiscordId? ParentId { get; }

	/// <summary>
	/// Gets the channel flags.
	/// </summary>
	ChannelFlags? Flags { get; }

	/// <summary>
	/// Gets the computed permissions for the invoking user in the channel.
	/// </summary>
	string? Permissions { get; }


}

