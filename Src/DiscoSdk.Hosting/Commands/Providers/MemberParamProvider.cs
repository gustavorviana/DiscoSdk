using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Commands.Providers;

internal class MemberParamProvider(ISdkContextProvider context) : IParamProvider<IMember>
{
    public async Task<IMember?> GetValueAsync()
    {
        return ContextGuard.Require<IMemberContext>(context)
            .Member
            ?? throw new InvalidOperationException("The member context is available, but the Member property is null.");
    }
}
