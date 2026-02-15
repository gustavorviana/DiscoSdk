namespace DiscoSdk;

public sealed class UnhandledErrorEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; } = exception;
}
