namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents an audio channel in a Discord guild.
/// This interface groups channels that support voice/audio communication.
/// </summary>
public interface IGuildAudioChannel : IGuildChannel
{
	/// <summary>
	/// Gets the bitrate (in bits) for this audio channel.
	/// </summary>
	int? Bitrate { get; }

	/// <summary>
	/// Gets the user limit of this audio channel.
	/// </summary>
	int? UserLimit { get; }

	/// <summary>
	/// Gets the voice region ID for this audio channel.
	/// </summary>
	string? RtcRegion { get; }
}

