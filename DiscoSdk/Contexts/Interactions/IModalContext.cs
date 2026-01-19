namespace DiscoSdk.Contexts.Interactions;

public interface IModalContext : IInteractionContext
{
    string CustomId { get; }
    IReadOnlyCollection<IModalOption> Fields { get; }

    IModalOption? GetField(string customId);
}