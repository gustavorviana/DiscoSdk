namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for stage channel operations.
/// </summary>
public interface IStageChannelManager : IChannelManager<IStageChannelManager>
{
	/// <summary>
	/// Sets the bitrate for the stage channel.
	/// </summary>
	/// <param name="bitrate">The bitrate in bits per second.</param>
	/// <returns>The current <see cref="IStageChannelManager"/> instance.</returns>
	IStageChannelManager SetBitrate(int bitrate);

	/// <summary>
	/// Sets the RTC region for the stage channel.
	/// </summary>
	/// <param name="regionId">The region ID, or null for automatic region selection.</param>
	/// <returns>The current <see cref="IStageChannelManager"/> instance.</returns>
	IStageChannelManager SetRegion(string? regionId);
}

