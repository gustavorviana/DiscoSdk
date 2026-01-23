using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class RootCommandOption(DiscoSdk.Models.InteractionOption option, IObjectConverter converter) : CommandOption(option.Name, option.Value, converter), IRootCommandOption
{
    public IReadOnlyCollection<ICommandOption> Options { get; }
        = option.Options == null ? [] : [.. option.Options.Select(x => new CommandOption(x.Name, x.Value, converter))];

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : class
        => GetOption(name)?.To(@default);

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : struct
        => GetOption(name)?.To(@default);

    public ICommandOption? GetOption(string name)
        => Options.FirstOrDefault(x => x.Name == name);
}