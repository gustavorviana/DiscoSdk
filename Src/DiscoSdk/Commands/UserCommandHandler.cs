using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Commands;

public abstract class UserContextMenuHandler
{
    protected IServiceProvider Services { get; private set; } = null!;
    protected IUserCommandContext Context { get; private set; } = null!;

    internal void Init(IServiceProvider services)
    {
        Services = services;
        var contextProvider = services.GetRequiredService<ISdkContextProvider>();
        Context = (IUserCommandContext)contextProvider.GetContext();
    }
}
