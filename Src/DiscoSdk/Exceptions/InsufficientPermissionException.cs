namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when an operation requires a permission that the bot does not have.
/// </summary>
public sealed class InsufficientPermissionException : UnauthorizedAccessException
{
	/// <summary>
	/// Gets the required permission that is missing.
	/// </summary>
	public string RequiredPermission { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class with a custom message.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="requiredPermission">The permission that is required for the operation.</param>
	public InsufficientPermissionException(string message, string requiredPermission)
		: base(message)
	{
		RequiredPermission = requiredPermission;
	}

	/// <summary>
	/// Creates an <see cref="InsufficientPermissionException"/> for a specific operation that requires a permission.
	/// </summary>
	/// <param name="requiredPermission">The permission that is required.</param>
	/// <param name="operation">The operation that was attempted.</param>
	/// <returns>A new instance of <see cref="InsufficientPermissionException"/>.</returns>
	public static InsufficientPermissionException Operation(string requiredPermission, string operation)
	{
		return new InsufficientPermissionException($"Cannot {operation}. The bot requires the {requiredPermission} permission.", operation);
	}
}
