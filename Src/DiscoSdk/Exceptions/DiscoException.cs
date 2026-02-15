namespace DiscoSdk.Exceptions;

/// <summary>
/// Represents the base exception type for errors thrown by DiscoSdk.
/// </summary>
public class DiscoException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoException"/> class.
    /// </summary>
    public DiscoException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DiscoException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public DiscoException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
