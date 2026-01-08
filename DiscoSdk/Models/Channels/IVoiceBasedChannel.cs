using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord channel that supports voice communication.
/// </summary>
public interface IVoiceBasedChannel : IChannel
{
	/// <summary>
	/// Gets the bitrate of the voice channel.
	/// </summary>
	int? Bitrate { get; }

	/// <summary>
	/// Gets the user limit of the voice channel.
	/// </summary>
	int? UserLimit { get; }

	/// <summary>
	/// Gets the voice region ID for the voice channel.
	/// </summary>
	string? RtcRegion { get; }

	/// <summary>
	/// Gets the camera video quality mode of the voice channel.
	/// </summary>
	VideoQualityMode? VideoQualityMode { get; }
}

