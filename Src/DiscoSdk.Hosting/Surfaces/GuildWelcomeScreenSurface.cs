using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildWelcomeScreenSurface(DiscordClient client, GuildWrapper guild) : IGuildWelcomeScreen
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly GuildWrapper _guild = guild ?? throw new ArgumentNullException(nameof(guild));

    public IRestAction<IWelcomeScreen> Get()
        => RestAction<IWelcomeScreen>.Create(async ct => await _client.GuildClient.GetWelcomeScreenAsync(_guild.Id, ct).ConfigureAwait(false));

    public IEditWelcomeScreenAction Edit() => new EditWelcomeScreenAction(_client, _guild);
}
