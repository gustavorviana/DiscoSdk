using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Logging;
using DiscoSdk.Models.JsonConverters;
using System.Globalization;
using System.Text.Json;

namespace DiscoSdk.Hosting.Builders;

/// <summary>
/// Fluent builder for <see cref="DiscordClient"/>.
/// </summary>
public class DiscordClientBuilder
{
	private readonly string _token;
	private DiscordIntent? _intents;
	private int? _totalShards;
	private int _eventProcessorMaxConcurrency = 0;
	private int _eventProcessorQueueCapacity = 100;
	private ILogger? _logger;
	private JsonSerializerOptions? _jsonOptions;
	private TimeSpan? _reconnectDelay;
	private IObjectConverter? _objectConverter;
	private GatewayCompressMode? _gatewayCompressMode;

	/// <summary>
	/// Initializes a new instance of <see cref="DiscordClientBuilder"/> with the required bot token.
	/// </summary>
	/// <param name="token">The Discord bot token.</param>
	/// <exception cref="ArgumentException">Thrown when the token is null or empty.</exception>
	public DiscordClientBuilder(string token)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentException("Token cannot be null or empty.", nameof(token));

		_token = token;
	}

	/// <summary>
	/// Creates a new instance of <see cref="DiscordClientBuilder"/> with the required bot token.
	/// </summary>
	/// <param name="token">The Discord bot token.</param>
	/// <returns>A new <see cref="DiscordClientBuilder"/> instance.</returns>
	/// <exception cref="ArgumentException">Thrown when the token is null or empty.</exception>
	public static DiscordClientBuilder Create(string token)
	{
		return new DiscordClientBuilder(token);
	}

	/// <summary>
	/// Sets the gateway intents to subscribe to.
	/// </summary>
	/// <param name="intents">The <see cref="DiscordIntent"/> flags to subscribe to.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	public DiscordClientBuilder WithIntents(DiscordIntent intents)
	{
		_intents = intents;
		return this;
	}

	/// <summary>
	/// Sets the total number of shards. If null, the value will be determined from the gateway.
	/// </summary>
	/// <param name="totalShards">The total number of shards, or null to auto-detect.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when totalShards is less than 1.</exception>
	public DiscordClientBuilder WithTotalShards(int? totalShards)
	{
		if (totalShards.HasValue && totalShards.Value < 1)
			throw new ArgumentOutOfRangeException(nameof(totalShards), "Total shards must be at least 1.");

		_totalShards = totalShards;
		return this;
	}

	/// <summary>
	/// Sets the maximum number of concurrent event processing operations.
	/// Uses the managed .NET ThreadPool (similar to ASP.NET Core MaxConcurrency).
	/// If 0 or negative, defaults to ProcessorCount * 2.
	/// </summary>
	/// <param name="maxConcurrency">The maximum number of concurrent operations. Use 0 or negative to use default.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	public DiscordClientBuilder WithEventProcessorMaxConcurrency(int maxConcurrency)
	{
		_eventProcessorMaxConcurrency = maxConcurrency;
		return this;
	}

	/// <summary>
	/// Sets the capacity of the bounded event processing queue.
	/// When the queue is full, producers will wait (backpressure) to prevent memory growth.
	/// Default is 100. Must be at least 1.
	/// </summary>
	/// <param name="capacity">The queue capacity. Must be at least 1.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is less than 1.</exception>
	public DiscordClientBuilder WithEventProcessorQueueCapacity(int capacity)
	{
		if (capacity < 1)
			throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be at least 1.");

		_eventProcessorQueueCapacity = capacity;
		return this;
	}

	/// <summary>
	/// Sets the logger instance. If null, uses NullLogger.
	/// </summary>
	/// <param name="logger">The logger instance to use.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	public DiscordClientBuilder WithLogger(ILogger? logger)
	{
		_logger = logger;
		return this;
	}

	/// <summary>
	/// Sets the JSON serializer options. If null, uses <see cref="DiscoJson.Create()"/>.
	/// </summary>
	/// <param name="jsonOptions">The JSON serializer options to use.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	public DiscordClientBuilder WithJsonOptions(JsonSerializerOptions? jsonOptions)
	{
		_jsonOptions = jsonOptions;
		return this;
	}

	/// <summary>
	/// Sets the delay before attempting to reconnect after a connection loss.
	/// Default is 5 seconds.
	/// </summary>
	/// <param name="delay">The delay before reconnecting. Must be non-negative.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when delay is negative.</exception>
	public DiscordClientBuilder WithReconnectDelay(TimeSpan delay)
	{
		if (delay < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(delay), "Reconnect delay cannot be negative.");

		_reconnectDelay = delay;
		return this;
	}

    /// <summary>
    /// Sets the object converter used to transform raw interaction values into strongly-typed objects.
    /// This converter is applied when resolving command arguments, modal fields, and other interaction data.
    /// </summary>
    /// <param name="converter">The converter responsible for mapping raw values to target types.</param>
    /// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="converter"/> is null.</exception>
    public DiscordClientBuilder WithObjectConverter(IObjectConverter converter)
    {
        if (converter is null)
            throw new ArgumentNullException(nameof(converter));

        _objectConverter = converter;
        return this;
    }

	/// <summary>
	/// Sets the gateway compression mode for WebSocket communication.
	/// Default is <see cref="GatewayCompressMode.ZlibStream"/>.
	/// </summary>
	/// <param name="compressMode">The compression mode to use for gateway communication.</param>
	/// <returns>The current <see cref="DiscordClientBuilder"/> instance.</returns>
	public DiscordClientBuilder WithGatewayCompressMode(GatewayCompressMode compressMode)
	{
		_gatewayCompressMode = compressMode;
		return this;
	}

    /// <summary>
    /// Builds the <see cref="DiscordClient"/> instance, starts the connection to Discord Gateway,
    /// and returns the client ready for use. The client will be connecting in the background.
    /// Use <see cref="DiscordClient.WaitReadyAsync(CancellationToken)"/> or <see cref="DiscordClient.WaitReadyAsync(TimeSpan
	/// )"/> to wait for the bot to be fully ready.
    /// </summary>
    /// <returns>A task that represents the asynchronous build operation. The result contains the configured and started <see cref="DiscordClient"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required properties (Intents) are not set.</exception>
    public async Task<DiscordClient> BuildAsync()
	{
		if (!_intents.HasValue)
			throw new InvalidOperationException("Intents are required. Use WithIntents() to set them.");

		var config = new DiscordClientConfig
		{
			Token = _token,
			Intents = _intents.Value,
			TotalShards = _totalShards,
			EventProcessorMaxConcurrency = _eventProcessorMaxConcurrency,
			EventProcessorQueueCapacity = _eventProcessorQueueCapacity,
			Logger = _logger,
			ReconnectDelay = _reconnectDelay ?? TimeSpan.FromSeconds(5),
			GatewayCompressMode = _gatewayCompressMode ?? GatewayCompressMode.ZlibStream
		};

		var jsonOptions = _jsonOptions ?? DiscoJson.Create();

		var builder = new DiscordClient(config, jsonOptions, _objectConverter ?? new ObjectConverter(CultureInfo.InvariantCulture));
		await builder.StartAsync();

		return builder;
	}
}

