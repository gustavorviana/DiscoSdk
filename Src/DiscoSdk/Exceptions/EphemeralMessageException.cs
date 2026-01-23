namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when attempting to perform an operation on an ephemeral message that is not supported.
/// </summary>
/// <remarks>
/// Ephemeral messages are only visible to the user who triggered the interaction and have limited operations available.
/// This exception is thrown when attempting to perform an operation that is not supported on ephemeral messages.
/// </remarks>
public sealed class EphemeralMessageException : InvalidOperationException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EphemeralMessageException"/> class.
	/// </summary>
	/// <param name="message">The error message.</param>
	private EphemeralMessageException(string message) : base(message)
	{
	}

	/// <summary>
	/// Creates an exception for an operation that cannot be performed on ephemeral messages.
	/// </summary>
	/// <param name="operation">The operation that was attempted (e.g., "edit", "delete", "pin").</param>
	/// <returns>A new instance of <see cref="EphemeralMessageException"/>.</returns>
	public static EphemeralMessageException Operation(string operation)
	{
		return new EphemeralMessageException($"Cannot {operation} an ephemeral message. Ephemeral messages are only visible to the user who triggered the interaction and have limited operations available.");
	}
}

