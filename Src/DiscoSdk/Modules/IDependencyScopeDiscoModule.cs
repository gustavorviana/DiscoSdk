using DiscoSdk.Contexts;

namespace DiscoSdk.Modules;

public interface IDependencyScopeDiscoModule : IDiscoModule
{
    Task OnScopeCreatedAsync(
        IContext context,
        IServiceProvider scopeServices,
        CancellationToken cancellationToken = default);
}
