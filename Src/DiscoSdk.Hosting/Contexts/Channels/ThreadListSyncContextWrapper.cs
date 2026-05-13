using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ThreadListSyncContextWrapper(DiscordClient client, IGuild guild, ImmutableArray<IGuildThreadChannel> threads)
	: ContextWrapper(client), IThreadListSyncContext
{
	public IGuild Guild => guild;
	public ImmutableArray<IGuildThreadChannel> Threads => threads;
}
