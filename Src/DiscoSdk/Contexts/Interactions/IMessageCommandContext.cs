using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Contexts.Interactions;

/// <summary>
/// Represents the context for a message context menu command interaction.
/// </summary>
public interface IMessageCommandContext : IInteractionContext
{
	/// <summary>
	/// Gets the name of the invoked message command.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the target message of the context menu command.
	/// </summary>
	IMessage TargetMessage { get; }

	/// <summary>
	/// Gets the resolved data from the interaction.
	/// </summary>
	IRestAction<IInteractionResolved?> Resolved { get; }
}
