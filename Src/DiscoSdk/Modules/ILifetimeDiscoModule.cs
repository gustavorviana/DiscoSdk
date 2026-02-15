namespace DiscoSdk.Modules;

public interface ILifetimeDiscoModule : IDiscoModule
{
    Task OnPreInitializeAsync(IDiscordClient discordClient);

    Task OnGatewayReadyAsync(IDiscordClient discordClient);

    Task OnShutdownAsync(IDiscordClient discordClient);
}