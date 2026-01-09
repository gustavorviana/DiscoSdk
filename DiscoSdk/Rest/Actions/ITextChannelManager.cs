using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for text channel operations.
/// </summary>
public interface ITextChannelManager : IMessageChannelManager<ITextChannelManager>
{
	/// <summary>
	/// Sets the topic of the text channel.
	/// </summary>
	/// <param name="topic">The new topic, or null to remove the topic.</param>
	/// <returns>The current <see cref="ITextChannelManager"/> instance.</returns>
	ITextChannelManager SetTopic(string? topic);

	/// <summary>
	/// Sets the default auto-archive duration for threads created in this channel.
	/// </summary>
	/// <param name="duration">The auto-archive duration.</param>
	/// <returns>The current <see cref="ITextChannelManager"/> instance.</returns>
	ITextChannelManager SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration duration);
}

