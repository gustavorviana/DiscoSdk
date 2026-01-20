using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildVoiceChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildVoiceChannelWrapper : GuildChannelWrapperBase, IGuildVoiceChannel
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GuildVoiceChannelWrapper"/> class.
	/// </summary>
	/// <param name="channel">The channel instance to wrap.</param>
	/// <param name="guild">The guild this channel belongs to.</param>
	/// <param name="client">The Discord client for performing operations.</param>
	public GuildVoiceChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
		: base(client, channel, guild)
	{
	}

	/// <inheritdoc />
	public int? Bitrate => _channel.Bitrate;

	/// <inheritdoc />
	public int? UserLimit => _channel.UserLimit;

	/// <inheritdoc />
	public string? RtcRegion => _channel.RtcRegion;

	/// <inheritdoc />
	public VideoQualityMode? VideoQualityMode => _channel.VideoQualityMode;

    public IVoiceChannelManager GetManager()
    {
		return new VoiceChannelManagerWrapper(Id, _client.ChannelClient);
    }
}