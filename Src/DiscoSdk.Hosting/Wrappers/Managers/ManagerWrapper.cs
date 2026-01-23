using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Managers;

/// <summary>
/// Base class for manager wrappers that track changes and can be reset.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class, used for method chaining.</typeparam>
internal abstract class ManagerWrapper<TSelf> : IRestAction, IManager<TSelf> where TSelf : ManagerWrapper<TSelf>, IManager<TSelf>
{
    private readonly HashSet<string> _modifiedKeys = [];

    /// <summary>
    /// Gets a value indicating whether this manager has any pending changes.
    /// </summary>
    public bool HasChanges => _modifiedKeys.Count > 0;

    /// <summary>
    /// Gets a read-only collection of keys that have been modified.
    /// </summary>
    public IReadOnlyCollection<string> ModifiedKeys => [.. _modifiedKeys];

    /// <summary>
    /// Marks a key as modified.
    /// </summary>
    /// <param name="key">The key to mark as modified.</param>
    protected void MarkAsModified(string key)
    {
        _modifiedKeys.Add(key);
    }

    /// <summary>
    /// Resets all changes, restoring the manager to its original state.
    /// </summary>
    /// <returns>The current <see cref="IManager{TSelf}"/> instance.</returns>
    public TSelf Reset()
    {
        _modifiedKeys.Clear();
        return (TSelf)this;
    }

    /// <summary>
    /// Resets a specific key, restoring it to its original value.
    /// </summary>
    /// <param name="key">The key to reset.</param>
    /// <returns>The current <see cref="IManager{TSelf}"/> instance.</returns>
    public TSelf Reset(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _modifiedKeys.Remove(key);
        return (TSelf)this;
    }

    /// <summary>
    /// Resets multiple keys, restoring them to their original values.
    /// </summary>
    /// <param name="keys">The keys to reset.</param>
    /// <returns>The current <see cref="IManager{TSelf}"/> instance.</returns>
    public TSelf Reset(params string[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys)
        {
            if (key != null)
                _modifiedKeys.Remove(key);
        }

        return (TSelf)this;
    }

    /// <inheritdoc />
    public void Execute()
    {
        ExecuteAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!HasChanges)
            return Task.CompletedTask;

        return ExecuteInternalAsync(cancellationToken);
    }

    /// <summary>
    /// Executes the manager's changes internally.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task ExecuteInternalAsync(CancellationToken cancellationToken);
}