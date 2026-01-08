using DiscoSdk.Models;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord direct message channel.
/// </summary>
public interface IDmChannel : IChannel
{
	/// <summary>
	/// Gets the recipients of the DM.
	/// </summary>
	User[]? Recipients { get; }

	/// <summary>
	/// Gets the ID of the last message sent in this channel.
	/// </summary>
	DiscordId? LastMessageId { get; }

	/// <summary>
	/// Gets when the last pinned message was pinned.
	/// </summary>
	string? LastPinTimestamp { get; }

	/// <summary>
	/// Gets the ID of the creator of the DM.
	/// </summary>
	DiscordId? OwnerId { get; }
}

