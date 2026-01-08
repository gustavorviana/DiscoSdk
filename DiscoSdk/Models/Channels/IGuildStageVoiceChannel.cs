using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord guild stage voice channel.
/// </summary>
public interface IGuildStageVoiceChannel : IGuildChannel, IVoiceBasedChannel
{
	/// <summary>
	/// Gets the topic of the stage channel.
	/// </summary>
	string? Topic { get; }

	/// <summary>
	/// Creates a builder for editing this channel.
	/// </summary>
	/// <returns>An <see cref="IEditGuildStageVoiceChannelRestAction"/> instance for editing the channel.</returns>
	IEditGuildStageVoiceChannelRestAction Edit();
}

