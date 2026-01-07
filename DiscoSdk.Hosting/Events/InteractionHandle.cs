namespace DiscoSdk.Hosting.Events
{
    internal class InteractionHandle(string id, string token)
    {
        public string Id => id;
        public string Token => token;
        public bool IsDeferred { get; set; }
        public bool Responded { get; set; }
    }
}
