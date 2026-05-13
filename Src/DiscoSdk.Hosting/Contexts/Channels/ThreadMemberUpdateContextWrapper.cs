using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class ThreadMemberUpdateContextWrapper(DiscordClient client, Snowflake threadId, Snowflake userId, IGuild guild)
	: ContextWrapper(client), IThreadMemberUpdateContext
{
	public Snowflake ThreadId => threadId;
	public Snowflake UserId => userId;
	public IGuild Guild => guild;
}
