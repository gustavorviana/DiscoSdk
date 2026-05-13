using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class StageInstanceContextWrapper(DiscordClient client, IStageInstance instance, IGuild guild)
	: ContextWrapper(client), IStageInstanceContext
{
	public IStageInstance Instance => instance;
	public IGuild Guild => guild;
}
