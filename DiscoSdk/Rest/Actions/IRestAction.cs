namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents an asynchronous REST operation that can be executed.
/// </summary>
public interface IRestAction
{
    /// <summary>
    /// Executes the action synchronously.
    /// This will block the current thread until the operation completes.
    /// </summary>
    /// <remarks>
    /// Use this method with caution as it blocks the thread. Prefer <see cref="ExecuteAsync"/> when possible.
    /// </remarks>
    void Execute();

    /// <summary>
    /// Executes the action asynchronously and returns the result.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the operation result.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an asynchronous REST operation that can be executed.
/// </summary>
/// <typeparam name="T">The type of the result returned by this action.</typeparam>
public interface IRestAction<T> : IRestAction
{
    /// <summary>
    /// Executes the action synchronously and returns the result.
    /// This will block the current thread until the operation completes.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <remarks>
    /// Use this method with caution as it blocks the thread. Prefer <see cref="ExecuteAsync"/> when possible.
    /// </remarks>
    new T Execute();

    /// <summary>
    /// Executes the action asynchronously and returns the result.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the operation result.</returns>
    new Task<T> ExecuteAsync(CancellationToken cancellationToken = default);
}