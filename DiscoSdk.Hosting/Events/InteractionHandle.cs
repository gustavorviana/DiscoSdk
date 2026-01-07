using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Events
{
    internal class InteractionHandle(DiscordId id, string token)
    {
        public DiscordId Id => id;
        public string Token => token;
        public bool IsDeferred { get; set; }
        public bool Responded { get; set; }
    }
}
