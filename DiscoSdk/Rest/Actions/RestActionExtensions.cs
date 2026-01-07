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
    /// Use this method with caution as it blocks the thread. Prefer <see cref="ExecuteAsync"/> when possible.
    /// </remarks>
    public static T Send<T>(this IRestAction<T> action)
    {
        return action.SendAsync().GetAwaiter().GetResult();
    }
}
