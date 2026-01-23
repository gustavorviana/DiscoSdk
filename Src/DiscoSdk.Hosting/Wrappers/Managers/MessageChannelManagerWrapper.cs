using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Base class for message channel manager wrappers.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class, used for method chaining.</typeparam>
internal abstract class MessageChannelManagerWrapper<TSelf> : ChannelManagerWrapper<TSelf>, IMessageChannelManager<TSelf> where TSelf : MessageChannelManagerWrapper<TSelf>, IMessageChannelManager<TSelf>
{
	protected MessageChannelManagerWrapper(Snowflake channelId, ChannelClient channelClient)
		: base(channelId, channelClient)
	{
	}

	/// <inheritdoc />
	public TSelf SetNsfw(bool nsfw)
	{
		_changes["nsfw"] = nsfw;
		MarkAsModified("nsfw");
		return (TSelf)this;
	}

	/// <inheritdoc />
	public TSelf SetRateLimitPerUser(Slowmode? slowmode)
	{
		_changes["rate_limit_per_user"] = slowmode?.Seconds ?? 0;
		MarkAsModified("rate_limit_per_user");
		return (TSelf)this;
	}
}