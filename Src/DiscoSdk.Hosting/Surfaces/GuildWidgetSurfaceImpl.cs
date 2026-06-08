using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildWidgetSurfaceImpl(DiscordClient client, GuildWrapper guild) : IGuildWidgetSurface
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly GuildWrapper _guild = guild ?? throw new ArgumentNullException(nameof(guild));

    public bool IsEnabled => _guild.Data.WidgetEnabled ?? false;
    public Snowflake? ChannelId => _guild.Data.WidgetChannelId;

    public IRestAction<IGuildWidget> Get()
        => RestAction<IGuildWidget>.Create(async ct => await _client.GuildClient.GetWidgetAsync(_guild.Id, ct).ConfigureAwait(false));

    public IEditGuildWidgetAction Edit() => new EditGuildWidgetAction(_client, _guild);

    public IRestAction<Stream> GetImage(string? style = null)
        => throw new NotSupportedException();
}
