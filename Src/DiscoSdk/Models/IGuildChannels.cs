using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild channel surface — every operation that targets <c>/guilds/:id/channels</c> and the
/// related role-as-routed reads (AFK / system / rules / public-updates pointers). Pre-bound to a
/// guild; reads come from the in-memory channel cache while creates/updates round-trip REST.
/// </summary>
public interface IGuildChannels
{
    /// <summary>ID of the AFK voice channel, or <c>null</c> when none is configured.</summary>
    Snowflake? AfkId { get; }

    /// <summary>AFK timeout in seconds, or <c>null</c> when not set.</summary>
    int? AfkTimeout { get; }

    /// <summary>ID of the system text channel where system messages are posted, or <c>null</c> when not configured.</summary>
    Snowflake? SystemId { get; }

    /// <summary>System-channel flags controlling which system messages are sent.</summary>
    SystemChannelFlags? SystemFlags { get; }

    /// <summary>ID of the rules text channel, or <c>null</c> when not configured.</summary>
    Snowflake? RulesId { get; }

    /// <summary>ID of the public-updates text channel where guild notices are posted, or <c>null</c> when not configured.</summary>
    Snowflake? PublicUpdatesId { get; }

    /// <summary>Builds a deferred REST action that creates a new channel in this guild.</summary>
    /// <param name="name">The channel name.</param>
    /// <param name="type">The channel type.</param>
    ICreateChannelAction Create(string name, ChannelType type);

    /// <summary>Returns a channel by its id from the in-memory cache; <c>null</c> if absent.</summary>
    IGuildChannelUnion? Get(Snowflake channelId);

    /// <summary>Returns every channel in this guild from the in-memory cache.</summary>
    IReadOnlyList<IGuildChannelUnion> GetAll();

    /// <summary>Returns every text channel in this guild from the in-memory cache.</summary>
    IReadOnlyList<IGuildTextChannel> GetText();

    /// <summary>Returns every voice channel in this guild from the in-memory cache.</summary>
    IReadOnlyList<IGuildVoiceChannel> GetVoice();

    /// <summary>Returns the guild's AFK voice channel, or <c>null</c> when none is configured.</summary>
    IGuildVoiceChannel? GetAfk();

    /// <summary>Returns the guild's system text channel, or <c>null</c> when none is configured.</summary>
    IGuildTextChannel? GetSystem();

    /// <summary>Returns the guild's rules text channel, or <c>null</c> when none is configured.</summary>
    IGuildTextChannel? GetRules();

    /// <summary>Returns the guild's public-updates text channel, or <c>null</c> when none is configured.</summary>
    IGuildTextChannel? GetPublicUpdates();

    /// <summary>
    /// Builds a deferred REST action that reorders channels in this guild. Each item specifies a
    /// channel id, its new position and optionally a new parent / lock_permissions flag.
    /// </summary>
    IReasonedRestAction ModifyPositions(IEnumerable<ChannelPosition> positions);

    /// <summary>
    /// Builds a deferred REST action that lists every non-archived thread in this guild the bot
    /// can see, across every parent channel.
    /// </summary>
    IRestAction<IReadOnlyList<IGuildThreadChannel>> ListActiveThreads();
}
