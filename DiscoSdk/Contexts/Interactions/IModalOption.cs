using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Interactions;

public interface IModalOption : IInteractionOption
{
    string CustomId { get; }
}