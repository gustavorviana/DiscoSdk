using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Messages;

/// <summary>
/// Context for the <c>MESSAGE_REACTION_REMOVE_EMOJI</c> Gateway event — every reaction with a
/// specific emoji on a single message was cleared.
/// </summary>
public interface IMessageReactionRemoveEmojiContext : IContext
{
	/// <summary>The message whose reactions were cleared.</summary>
	Snowflake MessageId { get; }

	/// <summary>The emoji whose reactions were cleared.</summary>
	IEmoji Emoji { get; }

	/// <summary>The channel the message is in.</summary>
	ITextBasedChannel Channel { get; }

	/// <summary>The guild the channel belongs to, or null for DMs.</summary>
	IGuild? Guild { get; }
}
