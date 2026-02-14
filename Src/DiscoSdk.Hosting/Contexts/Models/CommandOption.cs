using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class CommandOption(string name, object? value, IObjectConverter converter) : CommandOptionValue(value, converter), ICommandOption
{
    public string Name => name;
}
