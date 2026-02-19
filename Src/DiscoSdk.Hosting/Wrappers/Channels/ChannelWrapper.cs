using DiscoSdk.Hosting.Containers;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Utils;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Base wrapper that implements <see cref="IChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ChannelWrapper"/> class.
/// </remarks>
/// <param name="channel">The channel instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class ChannelWrapper(DiscordClient client, Channel channel) : IChannel
{
    protected readonly Channel _channel = channel ?? throw new ArgumentNullException(nameof(channel));
    protected readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public Snowflake Id => _channel.Id;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => _channel.Id.CreatedAt;

    /// <inheritdoc />
    public ChannelType Type => _channel.Type;

    /// <inheritdoc />
    public virtual string Name => _channel.Name ?? string.Empty;

    /// <inheritdoc />
    public virtual IRestAction Delete()
    {
        return RestAction.Create(cancellationToken => _client.ChannelClient.DeleteAsync(_channel.Id, cancellationToken));
    }

    public IPermissionContainer GetPermissionContainer()
    {
        return new ChannelPermissionContainer(_channel, _client);
    }

    /// <summary>
    /// Converts a channel to its most specific type based on the channel type.
    /// </summary>
    /// <param name="channel">The channel model to wrap.</param>
    /// <param name="guild">The guild this channel belongs to (null for DM channels).</param>
    /// <param name="client">The Discord client for performing operations.</param>
    /// <returns>The channel as its most specific type.</returns>
    public static IGuildChannelBase ToGuildChannel(Channel channel, IGuild? guild, DiscordClient client)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(client);

        if (!ChannelTypeUtils.IsGuild(channel.Type))
            throw new InvalidCastException();

        return (IGuildChannelBase)ToSpecificType(client, channel, guild);
    }

    /// <summary>
    /// Converts a channel to its most specific type based on the channel type.
    /// </summary>
    /// <param name="channel">The channel model to wrap.</param>
    /// <param name="guild">The guild this channel belongs to (null for DM channels).</param>
    /// <param name="client">The Discord client for performing operations.</param>
    /// <returns>The channel as its most specific type.</returns>
    public static IChannel ToSpecificType(DiscordClient client, Channel channel, IGuild? guild)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(client);

        if (channel.Type is ChannelType.Dm or ChannelType.GroupDm)
            return new DmChannelWrapper(client, channel);

        var unionWrapper = new GuildChannelUnionWrapper(client, channel, guild!);

        return unionWrapper.ToExpectedChannel() ?? unionWrapper;
    }

    internal void OnUpdate(Channel channel)
    {
    }
}