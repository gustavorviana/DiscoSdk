using DiscoSdk.Models.Enums;

namespace DiscoSdk.Utils;

/// <summary>
/// Utility class for channel type operations.
/// </summary>
public static class ChannelTypeUtils
{
	/// <summary>
	/// Determines if the specified channel type is a text channel.
	/// </summary>
	/// <param name="channelType">The channel type to check.</param>
	/// <returns>True if the channel type is a text channel; otherwise, false.</returns>
	/// <remarks>
	/// Text channels include:
	/// - <see cref="ChannelType.GuildText"/> (0)
	/// - <see cref="ChannelType.GuildAnnouncement"/> (5)
	/// </remarks>
	public static bool IsText(ChannelType channelType)
	{
		return channelType is ChannelType.GuildText or ChannelType.GuildAnnouncement;
	}

	/// <summary>
	/// Determines if the specified channel type is a voice channel.
	/// </summary>
	/// <param name="channelType">The channel type to check.</param>
	/// <returns>True if the channel type is a voice channel; otherwise, false.</returns>
	/// <remarks>
	/// Voice channels include:
	/// - <see cref="ChannelType.GuildVoice"/> (2)
	/// - <see cref="ChannelType.GuildStageVoice"/> (13)
	/// </remarks>
	public static bool IsVoice(ChannelType channelType)
	{
		return channelType is ChannelType.GuildVoice or ChannelType.GuildStageVoice;
	}

	/// <summary>
	/// Determines if the specified channel type is a guild channel.
	/// </summary>
	/// <param name="channelType">The channel type to check.</param>
	/// <returns>True if the channel type is a guild channel; otherwise, false.</returns>
	/// <remarks>
	/// Guild channels include all channel types except:
	/// - <see cref="ChannelType.Dm"/> (1)
	/// - <see cref="ChannelType.GroupDm"/> (3)
	/// </remarks>
	public static bool IsGuild(ChannelType channelType)
	{
		return channelType is not (ChannelType.Dm or ChannelType.GroupDm);
	}

    /// <summary>
    /// Determines if the specified channel type is a thread channel.
    /// </summary>
    /// <param name="channelType">The channel type to check.</param>
    /// <returns>True if the channel type is a guild channel; otherwise, false.</returns>
    /// <remarks>
    /// Guild channels include all channel types except:
    /// - <see cref="ChannelType.AnnouncementThread"/> (10)
    /// - <see cref="ChannelType.PrivateThread"/> (12)
    /// - <see cref="ChannelType.PublicThread"/> (11)
    /// </remarks>
    public static bool IsThread(ChannelType channelType)
	{
		return channelType is ChannelType.AnnouncementThread or ChannelType.PrivateThread or ChannelType.PublicThread;
    }
}