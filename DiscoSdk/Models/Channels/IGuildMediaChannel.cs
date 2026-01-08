using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord guild media channel.
/// </summary>
public interface IGuildMediaChannel : IGuildChannel, IThreadLikeChannel
{
	/// <summary>
	/// Creates a builder for editing this channel.
	/// </summary>
	/// <returns>An <see cref="IEditGuildMediaChannelRestAction"/> instance for editing the channel.</returns>
	IEditGuildMediaChannelRestAction Edit();
	/// <summary>
	/// Gets the default duration for newly created threads.
	/// </summary>
	int? DefaultAutoArchiveDuration { get; }

	/// <summary>
	/// Gets the topic of the media channel.
	/// </summary>
	string? Topic { get; }

	/// <summary>
	/// Gets the ID of the last message sent in this channel.
	/// </summary>
	DiscordId? LastMessageId { get; }

	/// <summary>
	/// Gets the rate limit per user for messages in this channel.
	/// </summary>
	int? RateLimitPerUser { get; }
}

