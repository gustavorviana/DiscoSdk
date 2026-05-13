using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Messages;

/// <summary>
/// Context for the <c>MESSAGE_REACTION_REMOVE_ALL</c> Gateway event — every reaction on a single
/// message was cleared at once.
/// </summary>
public interface IMessageReactionRemoveAllContext : IContext
{
	/// <summary>The message whose reactions were cleared.</summary>
	Snowflake MessageId { get; }

	/// <summary>The channel the message is in.</summary>
	ITextBasedChannel Channel { get; }

	/// <summary>The guild the channel belongs to, or null for DMs.</summary>
	IGuild? Guild { get; }
}
