using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Contexts.Interactions;

public interface IModalContext : IInteractionContext
{
    string CustomId { get; }
    IReadOnlyCollection<IModalOption> Options { get; }

    string? GetOption(string customId);

    IRestAction Acknowledge();
}