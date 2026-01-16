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
internal class ChannelWrapper(Channel channel, DiscordClient client) : IChannel
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

        return (IGuildChannelBase)ToSpecificType(channel, guild, client);
    }

    /// <summary>
    /// Converts a channel to its most specific type based on the channel type.
    /// </summary>
    /// <param name="channel">The channel model to wrap.</param>
    /// <param name="guild">The guild this channel belongs to (null for DM channels).</param>
    /// <param name="client">The Discord client for performing operations.</param>
    /// <returns>The channel as its most specific type.</returns>
    public static IChannel ToSpecificType(Channel channel, IGuild? guild, DiscordClient client)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(client);

        if (channel.Type is ChannelType.Dm or ChannelType.GroupDm)
            return new DmChannelWrapper(channel, client);

        var unionWrapper = new GuildChannelUnionWrapper(channel, guild!, client);

        return channel.Type switch
        {
            ChannelType.GuildText => unionWrapper.AsTextChannel() ?? (IChannel)unionWrapper,
            ChannelType.GuildAnnouncement => unionWrapper.AsNewsChannel() ?? (IChannel)unionWrapper,
            ChannelType.AnnouncementThread or ChannelType.PublicThread or ChannelType.PrivateThread => unionWrapper.AsThreadChannel() ?? (IChannel)unionWrapper,
            ChannelType.GuildVoice => unionWrapper.AsVoiceChannel() ?? (IChannel)unionWrapper,
            ChannelType.GuildStageVoice => unionWrapper.AsStageChannel() ?? (IChannel)unionWrapper,
            ChannelType.GuildForum => unionWrapper.AsForumChannel() ?? (IChannel)unionWrapper,
            ChannelType.GuildMedia => unionWrapper.AsMediaChannel() ?? (IChannel)unionWrapper,
            _ => unionWrapper
        };
    }
}