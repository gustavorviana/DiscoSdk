namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for voice channel operations.
/// </summary>
public interface IVoiceChannelManager : IChannelManager<IVoiceChannelManager>
{
	/// <summary>
	/// Sets the bitrate for the voice channel.
	/// </summary>
	/// <param name="bitrate">The bitrate in bits per second.</param>
	/// <returns>The current <see cref="IVoiceChannelManager"/> instance.</returns>
	IVoiceChannelManager SetBitrate(int bitrate);

	/// <summary>
	/// Sets the user limit for the voice channel.
	/// </summary>
	/// <param name="userLimit">The maximum number of users (0 for unlimited).</param>
	/// <returns>The current <see cref="IVoiceChannelManager"/> instance.</returns>
	IVoiceChannelManager SetUserLimit(int userLimit);

	/// <summary>
	/// Sets the RTC region for the voice channel.
	/// </summary>
	/// <param name="regionId">The region ID, or null for automatic region selection.</param>
	/// <returns>The current <see cref="IVoiceChannelManager"/> instance.</returns>
	IVoiceChannelManager SetRegion(string? regionId);
}

