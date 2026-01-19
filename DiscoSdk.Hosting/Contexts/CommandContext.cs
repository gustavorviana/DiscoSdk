using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Models.Interactions;

namespace DiscoSdk.Hosting.Contexts;

internal class CommandContext(IInteraction interaction, DiscordClient client) : InteractionContext(interaction, client), ICommandContext
{
    public string Name => Interaction.Data?.Name ?? string.Empty;

    public IReadOnlyCollection<IRootCommandOption> Options { get; }
            = interaction.Data?.Options == null ? [] : [.. interaction.Data.Options.Select(x => new RootCommandOption(x, client.ObjectConverter))];

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : class
        => GetOption(name)?.To(@default);

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : struct
        => GetOption(name)?.To(@default);

    public IRootCommandOption? GetOption(string name)
        => Options.FirstOrDefault(x => x.Name == name);
}