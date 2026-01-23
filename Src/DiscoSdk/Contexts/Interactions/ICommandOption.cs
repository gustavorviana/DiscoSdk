using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Interactions;

public interface ICommandOption : IInteractionOption
{
    string Name { get; }
}