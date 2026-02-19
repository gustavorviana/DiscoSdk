using DiscoSdk.Commands;
using DiscoSdk.Contexts;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Commands.Providers;

internal class UserParamProvider(ISdkContextProvider context) : IParamProvider<IUser>
{
    public async Task<IUser?> GetValueAsync()
    {
        return ContextGuard.Require<IUserContext>(context)
            .User
            ?? throw new InvalidOperationException("The user context is available, but the User property is null.");
    }
}
