using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildOnboardingSurfaceImpl(DiscordClient client, Snowflake guildId) : IGuildOnboardingSurface
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public IRestAction<IGuildOnboarding> Get()
        => RestAction<IGuildOnboarding>.Create(async ct =>
            new GuildOnboardingWrapper(_client, await _client.GuildTemplateClient.GetOnboardingAsync(guildId, ct).ConfigureAwait(false)));

    public IEditGuildOnboardingAction Edit() => new EditGuildOnboardingAction(_client, guildId);
}
