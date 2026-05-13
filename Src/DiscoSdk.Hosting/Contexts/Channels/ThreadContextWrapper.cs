using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ThreadContextWrapper(DiscordClient client, IGuildThreadChannel thread, IGuild guild)
	: ContextWrapper(client), IThreadContext
{
	public IGuildThreadChannel Thread => thread;
	public IGuild Guild => guild;
}
