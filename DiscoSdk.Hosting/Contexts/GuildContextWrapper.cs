using DiscoSdk.Contexts;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts
{
    internal class GuildContextWrapper(DiscordClient client, IGuild guild) : ContextWrapper(client), IGuildContext
    {
        public IGuild Guild => guild;
    }
}
