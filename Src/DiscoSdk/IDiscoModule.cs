using DiscoSdk.Commands;

namespace DiscoSdk;

public interface IDiscoModule
{
    Task OnPreInitializeAsync(IDiscordClient discordClient);

    Task OnGatewayReadyAsync(IDiscordClient discordClient);

    Task OnShutdownAsync(IDiscordClient discordClient);

    void OnCommandsUpdateWindowOpened(IDiscordClient discordClient, CommandContainer container);
}

