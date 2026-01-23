using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for news channel operations.
/// </summary>
public interface INewsChannelManager : IMessageChannelManager<INewsChannelManager>
{
	/// <summary>
	/// Sets the topic of the news channel.
	/// </summary>
	/// <param name="topic">The new topic, or null to remove the topic.</param>
	/// <returns>The current <see cref="INewsChannelManager"/> instance.</returns>
	INewsChannelManager SetTopic(string? topic);

	/// <summary>
	/// Sets the default auto-archive duration for threads created in this channel.
	/// </summary>
	/// <param name="duration">The auto-archive duration.</param>
	/// <returns>The current <see cref="INewsChannelManager"/> instance.</returns>
	INewsChannelManager SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration duration);
}

