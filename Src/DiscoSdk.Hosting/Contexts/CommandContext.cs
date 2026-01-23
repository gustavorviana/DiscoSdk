using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Wrappers;

namespace DiscoSdk.Hosting.Contexts;

internal class CommandContext(DiscordClient client, InteractionWrapper interaction) : InteractionContextWrapper(client, interaction), ICommandContext
{
    public string Name => Interaction.Data?.Name ?? string.Empty;

    public IReadOnlyCollection<IRootCommandOption> Options { get; }
            = interaction.Data?.Options == null ? [] : [.. interaction.Data.Options.Select(x => new RootCommandOption(x, client.ObjectConverter))];

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : class
        => GetOption(name)?.To(@default);

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : struct
        => GetOption(name)?.To(@default);

    public IRootCommandOption? GetOption(string name)
        => Options.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}