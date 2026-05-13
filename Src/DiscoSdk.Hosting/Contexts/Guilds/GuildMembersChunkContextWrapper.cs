using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildMembersChunkContextWrapper(DiscordClient client,
	IGuild guild,
	ImmutableArray<IMember> members,
	int chunkIndex,
	int chunkCount,
	string? nonce) : ContextWrapper(client), IGuildMembersChunkContext
{
	public IGuild Guild => guild;
	public ImmutableArray<IMember> Members => members;
	public int ChunkIndex => chunkIndex;
	public int ChunkCount => chunkCount;
	public string? Nonce => nonce;
}
