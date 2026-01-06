namespace DiscoSdk.Hosting.Gateway
{
    /// <summary>
    /// Represents a method that handles shard events without event arguments.
    /// </summary>
    /// <param name="sender">The shard that raised the event.</param>
    /// <returns>A task that represents the asynchronous event handling.</returns>
    internal delegate Task ShardEventHandler(Shard sender);

    /// <summary>
    /// Represents a method that handles shard events with event arguments.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event arguments.</typeparam>
    /// <param name="sender">The shard that raised the event.</param>
    /// <param name="arg">The event arguments.</param>
    /// <returns>A task that represents the asynchronous event handling.</returns>
    internal delegate Task ShardEventHandler<TEventArgs>(Shard sender, TEventArgs arg);
}
