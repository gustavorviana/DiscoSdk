namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Extension methods for <see cref="IRestAction{T}"/>.
/// </summary>
public static class RestActionExtensions
{
    /// <summary>
    /// Executes the action synchronously and returns the result.
    /// This will block the current thread until the operation completes.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    /// <remarks>
    /// Use this method with caution as it blocks the thread. Prefer <see cref="Execute"/> when possible.
    /// </remarks>
    public static T Execute<T>(this IRestAction<T> action)
    {
        return action.ExecuteAsync().GetAwaiter().GetResult();
    }
}
