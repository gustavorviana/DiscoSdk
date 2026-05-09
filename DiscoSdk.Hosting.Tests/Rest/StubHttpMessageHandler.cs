namespace DiscoSdk.Hosting.Tests.Rest;

/// <summary>
/// Minimal <see cref="HttpMessageHandler"/> for unit tests that records every request and
/// delegates response construction to a caller-supplied function.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;
    private int _requestCount;
    private readonly List<HttpRequestMessage> _requests = [];
    private readonly object _sync = new();

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        : this((req, _) => Task.FromResult(handler(req)))
    {
    }

    public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    public int RequestCount => Volatile.Read(ref _requestCount);

    public IReadOnlyList<HttpRequestMessage> Requests
    {
        get
        {
            lock (_sync)
                return _requests.ToArray();
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _requestCount);
        lock (_sync)
            _requests.Add(request);

        return _handler(request, cancellationToken);
    }
}
