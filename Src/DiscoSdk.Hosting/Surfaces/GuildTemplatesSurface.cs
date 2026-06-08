using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildTemplatesSurface(DiscordClient client, Snowflake guildId) : IGuildTemplates
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public IRestAction<IReadOnlyList<IGuildTemplate>> GetAll()
        => RestAction<IReadOnlyList<IGuildTemplate>>.Create(async ct =>
        {
            var templates = await _client.GuildTemplateClient.GetGuildTemplatesAsync(guildId, ct).ConfigureAwait(false);
            return templates.Select(t => (IGuildTemplate)new GuildTemplateWrapper(_client, t)).ToList().AsReadOnly();
        });

    public IRestAction<IGuildTemplate> Create(string name, string? description = null)
        => RestAction<IGuildTemplate>.Create(async ct =>
            new GuildTemplateWrapper(_client, await _client.GuildTemplateClient.CreateGuildTemplateAsync(guildId, name, description, ct).ConfigureAwait(false)));
}
