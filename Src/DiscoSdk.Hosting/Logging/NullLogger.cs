using DiscoSdk.Logging;

namespace DiscoSdk.Hosting.Logging;

/// <summary>
/// An empty logger that does nothing.
/// </summary>
public class NullLogger : ILogger
{
    /// <summary>
    /// Gets a singleton instance of the null logger.
    /// </summary>
    public static NullLogger Instance { get; } = new();

    private NullLogger() { }

    /// <inheritdoc />
    public void Log(LogLevel logLevel, string message, Exception? exception = null)
    {
        // Do nothing
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => false;
}

