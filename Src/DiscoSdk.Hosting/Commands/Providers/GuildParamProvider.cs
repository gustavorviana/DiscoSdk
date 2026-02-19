using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Commands.Providers;

internal class GuildParamProvider(ISdkContextProvider context) : IParamProvider<IGuild>
{
    public async Task<IGuild?> GetValueAsync()
    {
        return ContextGuard.Require<IGuildContext>(context)
            .Guild
            ?? throw new InvalidOperationException("The user context is available, but the User property is null.");
    }
}
