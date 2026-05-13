using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Context for the <c>CHANNEL_PINS_UPDATE</c> Gateway event — the pinned messages in a channel
/// changed.
/// </summary>
public interface IChannelPinsUpdateContext : IContext
{
	/// <summary>The channel whose pins changed.</summary>
	ITextBasedChannel Channel { get; }

	/// <summary>The guild the channel belongs to, or null for DMs.</summary>
	IGuild? Guild { get; }

	/// <summary>Time of the most recent pinned message, if any.</summary>
	DateTimeOffset? LastPinTimestamp { get; }
}
