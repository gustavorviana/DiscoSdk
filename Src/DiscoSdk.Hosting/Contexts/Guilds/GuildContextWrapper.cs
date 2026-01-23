using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds
{
    internal class GuildContextWrapper(DiscordClient client, IGuild guild) : ContextWrapper(client), IGuildContext
    {
        public IGuild Guild => guild;
    }
}
