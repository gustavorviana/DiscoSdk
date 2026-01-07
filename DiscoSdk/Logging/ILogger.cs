namespace DiscoSdk.Logging;

/// <summary>
/// Represents a type used to perform logging.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log.</param>
    void Log(LogLevel logLevel, string message, Exception? exception = null);

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns><c>true</c> if enabled; otherwise, <c>false</c>.</returns>
    bool IsEnabled(LogLevel logLevel);
}

