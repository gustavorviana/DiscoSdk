using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for message channel operations with type-safe method chaining.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class, used for method chaining.</typeparam>
public interface IMessageChannelManager<TSelf> : IChannelManager<TSelf> where TSelf : IMessageChannelManager<TSelf>
{
	/// <summary>
	/// Sets whether the channel is NSFW.
	/// </summary>
	/// <param name="nsfw">True if the channel is NSFW, false otherwise.</param>
	/// <returns>The current <see cref="IMessageChannelManager{TSelf}"/> instance.</returns>
	TSelf SetNsfw(bool nsfw);

	/// <summary>
	/// Sets the rate limit per user (slowmode) for the channel.
	/// </summary>
	/// <param name="slowmode">The slowmode duration, or null to disable slowmode.</param>
	/// <returns>The current <see cref="IMessageChannelManager{TSelf}"/> instance.</returns>
	TSelf SetRateLimitPerUser(Slowmode? slowmode);
}

