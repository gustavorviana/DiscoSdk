namespace DiscoSdk.Events
{
    public interface IDiscordEventDispatcher
    {
        void Register(IDiscordEventHandler handler);
    }
}