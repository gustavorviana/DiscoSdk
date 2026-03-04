using DiscoSdk.Models;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Contexts.Interactions;

/// <summary>
/// Represents the context for a user context menu command interaction.
/// </summary>
public interface IUserCommandContext : IInteractionContext
{
	/// <summary>
	/// Gets the name of the invoked user command.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the target user of the context menu command.
	/// </summary>
	IUser TargetUser { get; }

	/// <summary>
	/// Gets the target member of the context menu command, or null when invoked in DMs.
	/// </summary>
	IMember? TargetMember { get; }

	/// <summary>
	/// Gets the resolved data from the interaction.
	/// </summary>
	IRestAction<IInteractionResolved?> Resolved { get; }
}
