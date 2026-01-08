namespace DiscoSdk;

/// <summary>
/// Represents an object that can be deleted asynchronously.
/// </summary>
public interface IDeletable
{
	/// <summary>
	/// Deletes this object asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeleteAsync(CancellationToken cancellationToken = default);
}

