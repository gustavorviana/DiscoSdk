namespace DiscoSdk.Hosting;

/// <summary>
/// Internal implementation of <see cref="IDiscordClientAccessor"/>. Set once by
/// <see cref="Builders.DiscordClientBuilder.Build"/> after the <see cref="DiscordClient"/>
/// is constructed; throws on read until that happens.
/// </summary>
internal sealed class DiscordClientAccessor : IDiscordClientAccessor
{
    private IDiscordClient? _client;

    public IDiscordClient Client => _client!;

    /// <summary>Called by <see cref="Builders.DiscordClientBuilder.Build"/> once.</summary>
    internal void Set(IDiscordClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }
}
