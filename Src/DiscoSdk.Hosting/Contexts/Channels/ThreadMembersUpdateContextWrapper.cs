using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ThreadMembersUpdateContextWrapper(DiscordClient client,
	Snowflake threadId,
	IGuild guild,
	int memberCount,
	ImmutableArray<Snowflake> addedUserIds,
	ImmutableArray<Snowflake> removedUserIds) : ContextWrapper(client), IThreadMembersUpdateContext
{
	public Snowflake ThreadId => threadId;
	public IGuild Guild => guild;
	public int MemberCount => memberCount;
	public ImmutableArray<Snowflake> AddedUserIds => addedUserIds;
	public ImmutableArray<Snowflake> RemovedUserIds => removedUserIds;
}
