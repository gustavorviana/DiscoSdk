using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_MEMBERS_CHUNK</c> Gateway event — a (paginated) response to a previous
/// <c>Request Guild Members</c> command.
/// </summary>
public interface IGuildMembersChunkContext : IContext
{
	/// <summary>The guild the members belong to.</summary>
	IGuild Guild { get; }

	/// <summary>Members included in this chunk.</summary>
	ImmutableArray<IMember> Members { get; }

	/// <summary>Zero-based index of this chunk.</summary>
	int ChunkIndex { get; }

	/// <summary>Total number of chunks expected for the matching request.</summary>
	int ChunkCount { get; }

	/// <summary>The nonce supplied on the original <c>Request Guild Members</c>, if any.</summary>
	string? Nonce { get; }
}
