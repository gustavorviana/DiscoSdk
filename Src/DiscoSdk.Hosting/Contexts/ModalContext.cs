using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Rest.Actions;
using static DiscoSdk.Hosting.Rest.Clients.InteractionClient;

namespace DiscoSdk.Hosting.Contexts;

internal class ModalContext : InteractionContextWrapper, IModalContext
{
    public string CustomId => Interaction.Data?.CustomId ?? string.Empty;

    public IReadOnlyCollection<IModalOption> Options { get; }

    public ModalContext(DiscordClient client, InteractionWrapper interaction) : base(client, interaction)
    {
        var components = interaction.Data?.Components;
        Options = components == null ? [] : [.. components.SelectMany(x => x.Components ?? []).Select(x => new ModalOption(x.CustomId, x.GetValueString()))];
    }

    public string? GetOption(string customName) => Options.FirstOrDefault(x => x.CustomId.Equals(customName, StringComparison.OrdinalIgnoreCase))?.Value;

    public IRestAction Acknowledge()
    {
        return RestAction.Create(async cancellationToken =>
        {
            await Client
            .InteractionClient
            .AcknowledgeAsync(Interaction.Handle,
            AcknowledgeType.ModalSubmit,
            cancellationToken);
        });
    }
}