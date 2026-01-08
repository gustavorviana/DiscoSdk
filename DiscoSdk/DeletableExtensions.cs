namespace DiscoSdk;

/// <summary>
/// Extension methods for <see cref="IDeletable"/>.
/// </summary>
public static class DeletableExtensions
{
	/// <summary>
	/// Deletes this object synchronously.
	/// This will block the current thread until the operation completes.
	/// </summary>
	/// <param name="removable">The removable object to delete.</param>
	/// <remarks>
	/// Use this method with caution as it blocks the thread. Prefer <see cref="IDeletable.DeleteAsync"/> when possible.
	/// </remarks>
	public static void Delete(this IDeletable removable)
	{
		removable.DeleteAsync().GetAwaiter().GetResult();
	}
}

