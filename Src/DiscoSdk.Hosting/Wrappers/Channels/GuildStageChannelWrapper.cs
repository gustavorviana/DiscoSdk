using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildStageChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildStageChannelWrapper : GuildVoiceChannelWrapper, IGuildStageChannel
{
	public GuildStageChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
		: base(client, channel, guild)
	{
	}

	/// <inheritdoc />
	public IRestAction RequestToSpeak()
		=> RestAction.Create(async ct => await _client.GuildClient.RequestToSpeakAsync(Guild.Id, Id, ct));

	/// <inheritdoc />
	public IRestAction CancelRequestToSpeak()
		=> RestAction.Create(async ct => await _client.GuildClient.CancelRequestToSpeakAsync(Guild.Id, Id, ct));

	public new IStageChannelManager GetManager()
		=> new StageChannelManagerWrapper(Id, _client.ChannelClient);

	/// <inheritdoc />
	public IRestAction<IStageInstance> GetStageInstance()
		=> RestAction<IStageInstance>.Create(async ct =>
			(IStageInstance)new StageInstanceWrapper(_client, await _client.StageInstanceClient.GetAsync(Id, ct)));

	/// <inheritdoc />
	public ICreateStageInstanceAction CreateStageInstance(string topic)
		=> new CreateStageInstanceAction(_client, Id, topic);
}
