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
        // Discord wraps modal inputs in two layouts depending on the input type:
        //   ActionRow (type 1) → `components` array (used for TextInput)
        //   Label     (type 18) → `component` single (used for Checkbox / CheckboxGroup /
        //                        RadioGroup / FileUpload)
        // We flatten both shapes into a single ModalOption list so callers don't care.
        var rows = interaction.Data?.Components;
        Options = rows == null
            ? []
            : [.. rows.SelectMany(row =>
                {
                    if (row.Components is { Length: > 0 } actionRowChildren)
                        return actionRowChildren;
                    if (row.Component is { } labelChild)
                        return [labelChild];
                    return Array.Empty<DiscoSdk.Models.Interactions.ModalSubmitField>();
                })
                .Select(field => new ModalOption(field.CustomId, field.GetValueString()))];
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