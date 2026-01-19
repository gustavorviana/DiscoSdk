namespace DiscoSdk.Contexts.Interactions;

public interface ICommandContext : IInteractionContext, IWithOptionCollection<IRootCommandOption>
{
    string Name { get; }
}