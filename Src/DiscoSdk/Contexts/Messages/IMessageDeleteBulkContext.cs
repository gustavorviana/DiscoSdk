using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using System.Collections.Immutable;

namespace DiscoSdk.Contexts.Messages;

/// <summary>
/// Context for the <c>MESSAGE_DELETE_BULK</c> Gateway event — multiple messages were deleted at once
/// (e.g. via a bulk-delete admin action).
/// </summary>
public interface IMessageDeleteBulkContext : IContext
{
	/// <summary>The IDs of the deleted messages.</summary>
	ImmutableArray<Snowflake> Ids { get; }

	/// <summary>The channel the messages were in.</summary>
	ITextBasedChannel Channel { get; }

	/// <summary>The guild the channel belongs to, or null for DMs.</summary>
	IGuild? Guild { get; }
}
