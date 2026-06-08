using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildScheduledEventsSurface(DiscordClient client, Snowflake guildId) : IGuildScheduledEvents
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public IRestAction<IReadOnlyList<IGuildScheduledEvent>> GetAll(bool? withUserCount = null)
        => RestAction<IReadOnlyList<IGuildScheduledEvent>>.Create(async ct =>
        {
            var events = await _client.GuildScheduledEventClient.ListAsync(guildId, withUserCount, ct).ConfigureAwait(false);
            return events.Select(e => (IGuildScheduledEvent)new GuildScheduledEventWrapper(_client, e)).ToList().AsReadOnly();
        });

    public IRestAction<IGuildScheduledEvent> Get(Snowflake eventId, bool? withUserCount = null)
        => RestAction<IGuildScheduledEvent>.Create(async ct =>
            new GuildScheduledEventWrapper(_client, await _client.GuildScheduledEventClient.GetAsync(guildId, eventId, withUserCount, ct).ConfigureAwait(false)));

    public ICreateScheduledEventAction Create(string name, DateTimeOffset scheduledStartTime, ScheduledEventEntityType entityType)
        => new CreateScheduledEventAction(_client, guildId, name, scheduledStartTime, entityType);
}
