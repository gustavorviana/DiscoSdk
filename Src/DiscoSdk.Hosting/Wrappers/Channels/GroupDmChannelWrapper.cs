using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Requests.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGroupDmChannel"/> for a group direct-message <see cref="Channel"/>.
/// </summary>
internal class GroupDmChannelWrapper(DiscordClient client, Channel channel)
	: DmChannelWrapper(client, channel), IGroupDmChannel
{
	/// <inheritdoc />
	public Snowflake OwnerId => _channel.OwnerId ?? default;

	/// <inheritdoc />
	public IRestAction AddRecipient(Snowflake userId, string accessToken, string? nick = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

		var channelId = Id;
		var request = new GroupDmAddRecipientRequest
		{
			AccessToken = accessToken,
			Nick = nick,
		};

		return RestAction.Create(ct => _client.ChannelClient.AddGroupDmRecipientAsync(channelId, userId, request, ct));
	}

	/// <inheritdoc />
	public IRestAction RemoveRecipient(Snowflake userId)
	{
		var channelId = Id;
		return RestAction.Create(ct => _client.ChannelClient.RemoveGroupDmRecipientAsync(channelId, userId, ct));
	}
}
