using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents the base interface for all Discord channels.
/// </summary>
public interface IChannel
{
	/// <summary>
	/// Gets the unique identifier of the channel.
	/// </summary>
	DiscordId Id { get; }

	/// <summary>
	/// Gets the type of the channel.
	/// </summary>
	ChannelType Type { get; }
}

