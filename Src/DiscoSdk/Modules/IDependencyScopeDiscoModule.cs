namespace DiscoSdk.Modules;

public interface IDependencyScopeDiscoModule : IDiscoModule
{
    Task OnScopeCreatedAsync(
        IDiscordClient discordClient,
        IServiceProvider scopeServices,
        CancellationToken cancellationToken = default);

    Task OnScopeDisposingAsync(
        IDiscordClient discordClient,
        IServiceProvider scopeServices,
        CancellationToken cancellationToken = default);
}
