using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord channel that belongs to a guild.
/// </summary>
public interface IGuildChannelBase : IChannel, IWithNsfw
{
	/// <summary>
	/// Gets the guild this channel belongs to.
	/// </summary>
	/// <returns>The guild this channel belongs to.</returns>
	IGuild Guild { get; }

	/// <summary>
	/// Gets the permission container for this channel.
	/// </summary>
	/// <returns>The permission container for this channel.</returns>
	IPermissionContainer GetPermissionContainer();

	DiscordPermission GetPermission(IMember member);
}