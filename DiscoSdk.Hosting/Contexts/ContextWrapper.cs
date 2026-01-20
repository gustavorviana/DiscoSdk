using DiscoSdk.Contexts;

namespace DiscoSdk.Hosting.Contexts;

internal class ContextWrapper(DiscordClient client) : IContext
{
    public DiscordClient Client => client;

    IDiscordClient IContext.Client => Client;
}