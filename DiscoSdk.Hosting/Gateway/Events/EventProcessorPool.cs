using System.Threading.Channels;
using DiscoSdk.Hosting.Logging;
using DiscoSdk.Logging;

namespace DiscoSdk.Hosting.Gateway.Events;

/// <summary>
/// Thread-safe execution pool for processing events asynchronously:
/// - Producers only enqueue (fast, non-blocking)
/// - Workers consume from bounded queue
/// - Processing happens on ThreadPool with limited concurrency
/// - Bounded channel provides backpressure when overloaded
/// </summary>
/// <typeparam name="T">The type of items to process.</typeparam>
internal class EventProcessorPool<T> : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private Task[]? _workerTasks;
    private readonly Func<T, Task> _eventProcessor;
    private readonly Channel<T> _channel;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly int _maxConcurrency;
    private readonly int _workerCount;
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventProcessorPool{T}"/> class.
    /// </summary>
    /// <param name="maxConcurrency">The maximum number of concurrent event processing operations (MaxDegreeOfParallelism). Uses ThreadPool managed threads. Default is ProcessorCount * 2.</param>
    /// <param name="eventProcessor">The callback function to process each item.</param>
    /// <param name="logger">The logger instance. If null, uses NullLogger.</param>
    /// <param name="queueCapacity">The capacity of the bounded queue. Provides backpressure when full. Default is 1000.</param>
    public EventProcessorPool(int maxConcurrency, Func<T, Task> eventProcessor, ILogger? logger = null, int queueCapacity = 1000)
    {
        _maxConcurrency = maxConcurrency > 0 ? maxConcurrency : Environment.ProcessorCount * 2;
        _eventProcessor = eventProcessor ?? throw new ArgumentNullException(nameof(eventProcessor));
        _logger = logger ?? NullLogger.Instance;
        _concurrencyLimiter = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);
        
        _workerCount = _maxConcurrency;

        // Create bounded channel for backpressure (prevents memory leak under load)
        // Similar to ASP.NET Core request queue
        var options = new BoundedChannelOptions(queueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait, // Block producers when full
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        };
        _channel = Channel.CreateBounded<T>(options);
    }

    /// <summary>
    /// Enqueues an item for processing by workers.
    /// Fast, non-blocking operation (producers only enqueue).
    /// </summary>
    /// <param name="item">The item to enqueue for processing.</param>
    /// <returns><c>true</c> if the item was successfully enqueued; otherwise, <c>false</c>.</returns>
    public bool Enqueue(T item)
    {
        if (_disposed)
            return false;

        return _channel.Writer.TryWrite(item);
    }

    /// <summary>
    /// Enqueues an item for processing, waiting if the queue is full (backpressure).
    /// </summary>
    /// <param name="item">The item to enqueue for processing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the item is enqueued.</returns>
    public async Task<bool> EnqueueAsync(T item, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return false;

        try
        {
            await _channel.Writer.WriteAsync(item, cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Starts the event processor workers to consume events from the queue.
    /// Workers use the managed .NET ThreadPool (similar to ASP.NET Core).
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(EventProcessorPool<T>));

        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
        catch { }

        _cancellationTokenSource = new CancellationTokenSource();
        var cts = _cancellationTokenSource;

        _workerTasks = new Task[_workerCount];
        for (int i = 0; i < _workerCount; i++)
        {
            var workerId = i;
            _workerTasks[i] = Task.Run(async () =>
            {
                try
                {
                    await ProcessEventsAsync(workerId, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Event processor worker {workerId} crashed", ex);
                }
            }, cts.Token);
        }

        _logger.Log(LogLevel.Debug, $"Started {_workerCount} worker(s) with max concurrency {_maxConcurrency} (using managed ThreadPool).");
    }

    /// <summary>
    /// Stops the event processor workers gracefully.
    /// </summary>
    public async Task StopAsync()
    {
        if (_disposed)
            return;

        _channel.Writer.Complete();
        _cancellationTokenSource.Cancel();

        // Wait for all worker tasks to finish
        if (_workerTasks != null)
        {
            try
            {
                await Task.WhenAll(_workerTasks);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Error stopping event processor workers", ex);
            }
        }

        _workerTasks = null;
        _logger.Log(LogLevel.Information, "All event processor workers stopped.");
    }

    /// <summary>
    /// Processes items from the queue on a worker task (ThreadPool managed).
    /// Follows ASP.NET Core pipeline model: workers consume, process directly (no Task.Run per item).
    /// </summary>
    private async Task ProcessEventsAsync(int workerId, CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            await _concurrencyLimiter.WaitAsync(ct);
            
            try
            {
                await _eventProcessor(item);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error processing item on worker {workerId}", ex);
            }
            finally
            {
                _concurrencyLimiter.Release();
            }
        }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            StopAsync().GetAwaiter().GetResult();
            _cancellationTokenSource?.Dispose();
            _concurrencyLimiter?.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

