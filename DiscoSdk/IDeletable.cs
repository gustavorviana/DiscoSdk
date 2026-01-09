using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Represents an object that can be deleted asynchronously.
/// </summary>
/// <remarks>
/// All methods that perform server actions (create, update, delete) must return <see cref="IRestAction"/> or <see cref="IRestAction{T}"/>.
/// This allows for deferred execution and better control over when the action is performed.
/// </remarks>
public interface IDeletable
{
	/// <summary>
	/// Gets a REST action for deleting this object.
	/// </summary>
	/// <returns>A REST action that can be executed to delete this object.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction Delete();
}

