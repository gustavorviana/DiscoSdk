using DiscoSdk.Commands;
using DiscoSdk.Commands.Localization;
using DiscoSdk.Contexts;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Commands;
using DiscoSdk.Hosting.Commands.Providers;
using DiscoSdk.Hosting.Contexts;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Compression;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Modules;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace DiscoSdk.Hosting.Builders;

/// <summary>
/// Fluent builder for <see cref="DiscordClient"/>.
/// </summary>
public class DiscordClientBuilder
{
    private readonly DiscoFactory<IDiscordEventHandler> _eventHandlers = new();
    private readonly DiscoFactory<IDiscoModule> _modules = new();
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
    private TimeProvider? _timeProvider;
    private IGatewaySocketFactory? _socketFactory;
    private IDiscordRestClient? _restClient;
    private readonly ServiceCollection _services = new();
    private CommandRegistryBuilder? _commandRegistryBuilder;
    private bool _slashDispatcherRegistered;
    private bool _contextMenuDispatcherRegistered;
    private bool _autoRegisterModuleRegistered;

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

    public DiscordClientBuilder WithSlashCommands(params Assembly[] assemblies)
    {
        // Scanner runs once and dies — it just populates the builder + registers handler types.
        new SlashCommandScanner(assemblies).ApplyTo(GetOrCreateRegistryBuilder(), _services);

        EnsureSlashDispatcherRegistered();
        EnsureAutoRegisterModuleRegistered();
        return this;
    }

    public DiscordClientBuilder WithContextMenuCommands(params Assembly[] assemblies)
    {
        new ContextMenuCommandScanner(assemblies).ApplyTo(GetOrCreateRegistryBuilder(), _services);

        EnsureContextMenuDispatcherRegistered();
        EnsureAutoRegisterModuleRegistered();
        return this;
    }

    private CommandRegistryBuilder GetOrCreateRegistryBuilder()
    {
        if (_commandRegistryBuilder is not null)
            return _commandRegistryBuilder;

        _commandRegistryBuilder = new CommandRegistryBuilder();

        _services.AddSingleton(sp => _commandRegistryBuilder.Build());
        _services.AddSingleton<ICommandRegistry>(sp => sp.GetRequiredService<CommandRegistry>());
        return _commandRegistryBuilder;
    }

    private void EnsureSlashDispatcherRegistered()
    {
        if (_slashDispatcherRegistered) return;
        _slashDispatcherRegistered = true;
        _services.AddSingleton<SlashCommandDispatcher>();
        _eventHandlers.Add<SlashCommandDispatcher>();
    }

    private void EnsureContextMenuDispatcherRegistered()
    {
        if (_contextMenuDispatcherRegistered) return;
        _contextMenuDispatcherRegistered = true;
        _services.AddSingleton<ContextMenuCommandDispatcher>();
        _eventHandlers.Add<ContextMenuCommandDispatcher>();
    }

    private void EnsureAutoRegisterModuleRegistered()
    {
        if (_autoRegisterModuleRegistered) return;
        _autoRegisterModuleRegistered = true;
        _services.AddSingleton<CommandAutoRegisterModule>();
        _modules.Add<CommandAutoRegisterModule>();
    }

    public DiscordClientBuilder AddModule(IDiscoModule module)
    {
        _modules.AddInstance(module);

        return this;
    }

    public DiscordClientBuilder AddModule<T>() where T : class, IDiscoModule
    {
        _modules.Add<T>();

        return this;
    }

    public DiscordClientBuilder AddModule(Type type)
    {
        _modules.Add(type);

        return this;
    }

    public DiscordClientBuilder AddEventHandler(IDiscordEventHandler module)
    {
        _eventHandlers.AddInstance(module);

        return this;
    }

    public DiscordClientBuilder AddEventHandler<T>() where T : class, IDiscordEventHandler
    {
        _eventHandlers.Add<T>();

        return this;
    }

    public DiscordClientBuilder AddEventHandler(Type type)
    {
        _eventHandlers.Add(type);

        return this;
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
    /// Sets the <see cref="TimeProvider"/> used by gateway heartbeat / reconnect logic and the REST
    /// rate-limit windows. Default is <see cref="TimeProvider.System"/>. Tests inject a
    /// <c>FakeTimeProvider</c> here to fast-forward virtual time without real sleeps.
    /// </summary>
    public DiscordClientBuilder WithTimeProvider(TimeProvider? timeProvider)
    {
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="IGatewaySocketFactory"/> used by each shard. Default is the production
    /// <see cref="DefaultGatewaySocketFactory"/>. Tests inject a factory that returns a controllable
    /// fake socket so the shard can be driven with arbitrary inbound frames.
    /// </summary>
    internal DiscordClientBuilder WithGatewaySocketFactory(IGatewaySocketFactory? socketFactory)
    {
        _socketFactory = socketFactory;
        return this;
    }

    /// <summary>
    /// Replaces the REST transport (<see cref="IDiscordRestClient"/>) used by every per-resource
    /// REST client (<c>ChannelClient</c>, <c>MessageClient</c>, etc.). Default is a real
    /// <c>DiscordRestClient</c> built against <c>https://discord.com/api/v10</c>. Tests inject a
    /// substituted instance to assert on outbound calls and stub responses without touching the
    /// network.
    /// </summary>
    internal DiscordClientBuilder WithRestClient(IDiscordRestClient? restClient)
    {
        _restClient = restClient;
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
    /// Registers an <see cref="ICommandLocalizationProvider"/> that supplies translated names
    /// and descriptions for every command, option, and choice at registration time. Manual
    /// localizations set on the builder are preserved.
    /// </summary>
    /// <typeparam name="TProvider">Concrete provider type; resolved from DI as a singleton.</typeparam>
    public DiscordClientBuilder WithCommandLocalization<TProvider>()
        where TProvider : class, ICommandLocalizationProvider
    {
        _services.AddSingleton<ICommandLocalizationProvider, TProvider>();
        return this;
    }

    /// <summary>
    /// Registers an explicit <see cref="ICommandLocalizationProvider"/> instance.
    /// </summary>
    public DiscordClientBuilder WithCommandLocalization(ICommandLocalizationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _services.AddSingleton(provider);
        return this;
    }

    public DiscordClientBuilder WithParamProvider<TProvider>()
            where TProvider : class, IParamProvider
    {
        var providerType = typeof(TProvider);

        // Ensure the incoming provider implements at least one IParamProvider<T>.
        var typedInterfaces = providerType
            .GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IParamProvider<>))
            .ToHashSet();

        if (typedInterfaces.Count == 0)
            throw new InvalidOperationException(
                $"{providerType.FullName} must implement at least one {typeof(IParamProvider<>).FullName} interface.");

        // Register each IParamProvider<T> interface as scoped -> TProvider
        // plus the concrete provider itself (optional but useful).
        _services.AddScoped(providerType);

        foreach (var @interface in typedInterfaces)
            _services.AddScoped(@interface, providerType);

        return this;
    }

    /// <summary>
    /// Builds the <see cref="DiscordClient"/> instance, starts the connection to Discord Gateway,
    /// and returns the client ready for use. The client will be connecting in the background.
    /// Use <see cref="DiscordClient.WaitReadyAsync(CancellationToken)"/> or <see cref="DiscordClient.WaitReadyAsync(TimeSpan)"/> to wait for the bot to be fully ready.
    /// </summary>
    /// <returns>A task that represents the asynchronous build operation. The result contains the configured and started <see cref="DiscordClient"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required properties (Intents) are not set.</exception>
    public DiscordClient Build()
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
            ReconnectDelay = _reconnectDelay ?? TimeSpan.FromSeconds(5),
            GatewayCompressMode = _gatewayCompressMode ?? GatewayCompressMode.ZlibStream,
        };

        var jsonOptions = _jsonOptions ?? DiscoJson.Create();

        WithParamProvider<ChannelParamProvider>();
        WithParamProvider<MemberParamProvider>();
        WithParamProvider<GuildParamProvider>();
        WithParamProvider<UserParamProvider>();

        var timeProvider = _timeProvider ?? TimeProvider.System;
        var socketFactory = _socketFactory ?? new DefaultGatewaySocketFactory(new GatewayDecompressFactory(config.GatewayCompressMode));
        var logger = _logger ?? NullLogger.Instance;
        var restClient = _restClient ?? new DiscordRestClient(
            _token,
            new Uri("https://discord.com/api/v10"),
            jsonOptions,
            logger,
            timeProvider);

        var objectConverter = _objectConverter ?? new ObjectConverter(CultureInfo.InvariantCulture);

        _services.AddSingleton(config)
            .AddSingleton(jsonOptions)
            .AddSingleton(objectConverter)
            .AddSingleton(logger)
            .AddSingleton(timeProvider)
            .AddSingleton(socketFactory)
            .AddSingleton(restClient)
            .AddSingleton<DiscordClientAccessor>()
            .AddSingleton<IDiscordClientAccessor>(sp => sp.GetRequiredService<DiscordClientAccessor>())
            .AddSingleton(sp => sp.GetRequiredService<IDiscordClientAccessor>().Client)
            .AddSingleton<CommandUpdateFactory>()
            .AddSingleton<ICommandUpdateFactory>(sp => sp.GetRequiredService<CommandUpdateFactory>())
            .AddSingleton<IGuildCommandUpdateFactory>(sp => sp.GetRequiredService<CommandUpdateFactory>())
            .AddScoped<SdkContextProvider>()
            .AddScoped<ISdkContextProvider>(svc => svc.GetRequiredService<SdkContextProvider>());

        var serviceProvider = _services.BuildServiceProvider();

        var client = new DiscordClient(serviceProvider,
            config,
            timeProvider,
            socketFactory,
            jsonOptions,
            logger,
            restClient,
            objectConverter
        );
        
        serviceProvider.GetRequiredService<DiscordClientAccessor>().Set(client);

        client.InternalInit(_modules.Build(serviceProvider), _eventHandlers.Build(serviceProvider));

        return client;
    }
}