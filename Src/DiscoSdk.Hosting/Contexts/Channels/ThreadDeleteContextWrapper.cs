using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ThreadDeleteContextWrapper(DiscordClient client, Snowflake threadId, Snowflake? parentId, IGuild guild)
	: ContextWrapper(client), IThreadDeleteContext
{
	public Snowflake ThreadId => threadId;
	public Snowflake? ParentId => parentId;
	public IGuild Guild => guild;
}
