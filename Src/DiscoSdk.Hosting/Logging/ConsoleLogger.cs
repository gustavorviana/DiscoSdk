using DiscoSdk.Logging;

namespace DiscoSdk.Hosting.Logging;

/// <summary>
/// A logger that writes to the console.
/// </summary>
public class ConsoleLogger : ILogger
{
    private readonly LogLevel _minLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
    /// </summary>
    /// <param name="minLevel">The minimum log level to output. Messages below this level will be ignored.</param>
    public ConsoleLogger(LogLevel minLevel = LogLevel.Information)
    {
        _minLevel = minLevel;
    }

    /// <inheritdoc />
    public void Log(LogLevel logLevel, string message, Exception? exception = null)
    {
        if (!IsEnabled(logLevel))
            return;

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var levelString = GetLevelString(logLevel);
        var prefix = $"[{timestamp}] [{levelString}]";

        Console.WriteLine($"{prefix} {message}");

        if (exception != null)
        {
            Console.WriteLine($"{prefix} Exception: {exception.GetType().Name}");
            Console.WriteLine($"{prefix} Message: {exception.Message}");
            if (exception.StackTrace != null)
            {
                Console.WriteLine($"{prefix} StackTrace: {exception.StackTrace}");
            }
        }
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLevel && logLevel != LogLevel.None;
    }

    private static string GetLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            _ => "UNKNOWN"
        };
    }
}

