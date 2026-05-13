using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class StageInstanceContextWrapper(DiscordClient client, StageInstance instance, IGuild guild)
	: ContextWrapper(client), IStageInstanceContext
{
	public StageInstance Instance => instance;
	public IGuild Guild => guild;
}
