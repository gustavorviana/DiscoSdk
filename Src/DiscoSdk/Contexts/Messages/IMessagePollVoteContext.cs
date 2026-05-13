using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Messages;

/// <summary>
/// Context for <c>MESSAGE_POLL_VOTE_ADD</c> and <c>MESSAGE_POLL_VOTE_REMOVE</c> Gateway events — a
/// user voted (or removed their vote) on a poll attached to a message.
/// </summary>
public interface IMessagePollVoteContext : IContext
{
	/// <summary>The user who cast or removed the vote.</summary>
	Snowflake UserId { get; }

	/// <summary>The message containing the poll.</summary>
	Snowflake MessageId { get; }

	/// <summary>The poll answer's ID (per-message, integer assigned by Discord).</summary>
	int AnswerId { get; }

	/// <summary>The channel the message is in.</summary>
	ITextBasedChannel Channel { get; }

	/// <summary>The guild the channel belongs to, or null for DM polls.</summary>
	IGuild? Guild { get; }
}
