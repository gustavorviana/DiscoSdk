namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager that tracks changes and can be reset to its original state, with type-safe method chaining.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class, used for method chaining.</typeparam>
public interface IManager<TSelf> where TSelf : IManager<TSelf>
{
    /// <summary>
    /// Resets all changes, restoring the manager to its original state.
    /// </summary>
    /// <returns>The current <see cref="IManager{TSelf}"/> instance.</returns>
    TSelf Reset();

    /// <summary>
    /// Resets a specific key, restoring it to its original value.
    /// </summary>
    /// <param name="key">The key to reset.</param>
    /// <returns>The current <see cref="IManager{TSelf}"/> instance.</returns>
    TSelf Reset(string key);

    /// <summary>
    /// Resets multiple keys, restoring them to their original values.
    /// </summary>
    /// <param name="keys">The keys to reset.</param>
    /// <returns>The current <see cref="IManager{TSelf}"/> instance.</returns>
    TSelf Reset(params string[] keys);
}