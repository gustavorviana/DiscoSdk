using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a channel that supports video quality settings.
/// </summary>
public interface IVideoChannel : IGuildAudioChannel
{
	/// <summary>
	/// Gets the camera video quality mode of this channel.
	/// </summary>
	VideoQualityMode? VideoQualityMode { get; }
}

