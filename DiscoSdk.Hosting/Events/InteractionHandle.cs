using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Events
{
    internal class InteractionHandle(Snowflake id, string token)
    {
        public Snowflake Id => id;
        public string Token => token;
        public bool IsDeferred { get; set; }
        public bool Responded { get; set; }
    }
}
