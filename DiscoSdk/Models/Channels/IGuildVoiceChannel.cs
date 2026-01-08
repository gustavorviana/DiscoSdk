using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord guild voice channel.
/// </summary>
public interface IGuildVoiceChannel : IGuildChannel, IVoiceBasedChannel
{
	/// <summary>
	/// Creates a builder for editing this channel.
	/// </summary>
	/// <returns>An <see cref="IEditGuildVoiceChannelRestAction"/> instance for editing the channel.</returns>
	IEditGuildVoiceChannelRestAction Edit();
}

