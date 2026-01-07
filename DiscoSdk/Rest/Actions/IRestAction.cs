namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents an asynchronous REST operation that can be executed.
/// </summary>
/// <typeparam name="T">The type of the result returned by this action.</typeparam>
public interface IRestAction<T>
{
	/// <summary>
	/// Executes the action asynchronously and returns the result.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The result contains the operation result.</returns>
	Task<T> SendAsync(CancellationToken cancellationToken = default);
}

