using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildStageChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildStageChannelWrapper : GuildVoiceChannelWrapper, IGuildStageChannel
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GuildStageChannelWrapper"/> class.
	/// </summary>
	/// <param name="channel">The channel instance to wrap.</param>
	/// <param name="guild">The guild this channel belongs to.</param>
	/// <param name="client">The Discord client for performing operations.</param>
	public GuildStageChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
		: base(client, channel, guild)
	{
	}

	/// <inheritdoc />
	public IRestAction RequestToSpeak()
	{
		return RestAction.Create(async cancellationToken =>
		{
			await _client.GuildClient.RequestToSpeakAsync(Guild.Id, Id, cancellationToken);
		});
	}

	/// <inheritdoc />
	public IRestAction CancelRequestToSpeak()
	{
		return RestAction.Create(async cancellationToken =>
		{
			await _client.GuildClient.CancelRequestToSpeakAsync(Guild.Id, Id, cancellationToken);
		});
	}

    public new IStageChannelManager GetManager()
    {
        return new StageChannelManagerWrapper(Id, _client.ChannelClient);
    }
}