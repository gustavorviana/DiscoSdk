namespace DiscoSdk.Exceptions;

/// <summary>
/// Exception thrown when an operation is attempted on an ephemeral message that is not supported.
/// </summary>
/// <remarks>
/// Ephemeral messages cannot be edited, deleted, crossposted, or have reactions added/removed.
/// </remarks>
public sealed class EphemeralMessageException : InvalidOperationException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EphemeralMessageException"/> class with a custom message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public EphemeralMessageException(string? message) : base(message)
	{
	}

    /// <summary>
    /// Initializes a new instance of the <see cref="EphemeralMessageException"/> class.
    /// </summary>
    /// <param name="operation">The operation that was attempted.</param>
    public static EphemeralMessageException Operation(string operation)
	{
		return new EphemeralMessageException($"Cannot {operation} an ephemeral message. Ephemeral messages cannot be modified after being sent.");
    }
}

