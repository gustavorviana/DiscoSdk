namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action that can retrieve multiple pages of results.
/// </summary>
/// <typeparam name="T">The type of items being paginated.</typeparam>
public interface IPaginationAction<TItem, TSelf> : IRestAction<TItem[]>
    where TSelf : IPaginationAction<TItem, TSelf>
{
    /// <summary>
    /// Sets the limit for the number of items to retrieve.
    /// </summary>
    /// <param name="limit">The maximum number of items to retrieve.</param>
    /// <returns>The current <see cref="IPaginationAction{T}"/> instance.</returns>
    TSelf Limit(int limit);
}