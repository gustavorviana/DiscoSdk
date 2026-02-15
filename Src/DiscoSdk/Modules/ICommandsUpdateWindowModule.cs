using DiscoSdk.Commands;

namespace DiscoSdk.Modules
{
    public interface ICommandsUpdateWindowModule : IDiscoModule
    {
        void OnCommandsUpdateWindowOpened(IDiscordClient discordClient, CommandContainer container);
    }
}
