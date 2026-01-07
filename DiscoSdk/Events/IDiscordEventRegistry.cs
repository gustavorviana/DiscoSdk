namespace DiscoSdk.Events
{
    public interface IDiscordEventRegistry
    {
        void Add(IDiscordEventHandler handler);
    }
}