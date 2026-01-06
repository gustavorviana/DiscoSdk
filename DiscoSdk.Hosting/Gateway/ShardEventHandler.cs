namespace DiscoSdk.Hosting.Gateway
{
    internal delegate Task ShardEventHandler(Shard sender);
    internal delegate Task ShardEventHandler<TEventArgs>(Shard sender, TEventArgs arg);
}
