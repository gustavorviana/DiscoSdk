namespace DiscoSdk.Contexts.Interactions;

public interface ICommandContext : IInteractionContext, IWithOptionCollection<IRootCommandOption>
{
    string Name { get; }
    string? Subcommand { get; }
    string? SubcommandGroup { get; }
}