using DiscoSdk.Events;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Hosting.Gateway.Payloads.Models;
using DiscoSdk.Hosting.Logging;
using DiscoSdk.Hosting.Managers;
using DiscoSdk.Hosting.Repositories;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using System.Text.Json;

namespace DiscoSdk.Hosting
{
    /// <summary>
    /// Main client for connecting to and managing Discord Gateway connections.
    /// </summary>
    public class DiscordClient : IDiscordClient
    {
        private readonly EventProcessorPool<ReceivedGatewayMessage> _eventProcessorPool;
        private readonly ManualResetEventSlim _shutdownEvent = new(false);
        private readonly ManualResetEventSlim _readyEvent = new(false);
        private readonly DiscordEventDispatcher _eventDispatcher;
        public IDiscordRestClient HttpClient { get; }
        private readonly DiscordClientConfig _config;
        public GuildManager Guilds { get; }
        internal ChannelManager Channels { get; }

        /// <summary>
        /// Gets the gateway intents configured for this client.
        /// </summary>
        public DiscordIntent Intents => _config.Intents;
        private readonly List<Shard> _shards = [];
        private bool _isInitialized = false;
        private int _totalShards = 0;
        private IdentifyGate? _gate;
        private bool _isShuttingDown = false;

        /// <summary>
        /// Gets the JSON serializer options used for deserializing Gateway events.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; }
        public IObjectConverter ObjectConverter { get; }

        public ILogger Logger { get; }

        /// <summary>
        /// Event raised when all shards are ready and the client is fully connected.
        /// </summary>
        public event EventHandler? OnReady;

        /// <summary>
        /// Event raised when the connection to Discord is lost.
        /// </summary>
        public event EventHandler? OnConnectionLost;

        /// <summary>
        /// Gets the total number of shards being used.
        /// </summary>
        public int TotalShards => _totalShards;

        /// <summary>
        /// Gets the event dispatcher for handling Gateway events.
        /// </summary>
        public IDiscordEventRegistry EventRegistry => _eventDispatcher;

        internal InteractionClient InteractionClient { get; }
        internal MessageClient MessageClient { get; }
        internal ChannelClient ChannelClient { get; }
        internal InviteClient InviteClient { get; }
        internal RoleClient RoleClient { get; }
        internal GuildClient GuildClient { get; }
        internal UserRepository Users { get; }
        internal DmChannelRepository DmRepository { get; }

        /// <summary>
        /// Creates a new command update action that allows queuing commands and registering them all at once.
        /// </summary>
        /// <returns>A new <see cref="CommandUpdateAction"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when ApplicationId is not yet available.</exception>
        public ICommandUpdateAction UpdateCommands()
        {
            if (ApplicationId == null)
                throw new InvalidOperationException("ApplicationId is not available yet. Wait for the bot to be ready or provide the ApplicationId manually.");

            return new CommandUpdateAction(this);
        }

        /// <summary>
        /// Gets or sets the application ID of the bot.
        /// </summary>
        public Snowflake? ApplicationId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether all shards are ready.
        /// </summary>
        public bool IsReady => _shards.All(s => s.Status == ShardStatus.Ready);

        /// <summary>
        /// Gets a value indicating whether the bot has fully initialized (all guilds have been loaded).
        /// </summary>
        public bool IsFullyInitialized => Guilds.IsFullyInitialized;

        /// <summary>
        /// Gets the current authenticated user.
        /// </summary>
        public ICurrentUser BotUser { get; private set; } = new ReadyUser();

        public DiscordClient(DiscordClientConfig config, JsonSerializerOptions jsonOptions, IObjectConverter converter)
        {
            _config = config;
            SerializerOptions = jsonOptions;
            Logger = config.Logger ?? NullLogger.Instance;
            _eventDispatcher = new DiscordEventDispatcher(this);
            HttpClient = new DiscordRestClient(config.Token, new Uri("https://discord.com/api/v10"), jsonOptions);
            InteractionClient = new InteractionClient(this);
            MessageClient = new MessageClient(HttpClient);
            ChannelClient = new ChannelClient(HttpClient, MessageClient);
            InviteClient = new InviteClient(HttpClient);
            RoleClient = new RoleClient(HttpClient);
            GuildClient = new GuildClient(HttpClient);
            Users = new UserRepository(this);
            Guilds = new GuildManager(this, Logger);
            Channels = new ChannelManager(this);
            DmRepository = new DmChannelRepository(this);
            ObjectConverter = converter;

            var maxConcurrency = config.EventProcessorMaxConcurrency > 0
                ? config.EventProcessorMaxConcurrency
                : Environment.ProcessorCount * 2;

            var queueCapacity = Math.Max(1, config.EventProcessorQueueCapacity);
            _eventProcessorPool = new EventProcessorPool<ReceivedGatewayMessage>(maxConcurrency, async (item) =>
            {
                await _eventDispatcher.ProcessEventAsync(item);
            }, Logger, queueCapacity);
        }

        /// <summary>
        /// Starts the Discord client and establishes connections to the Gateway.
        /// This method returns immediately after starting the connection process.
        /// Use <see cref="WaitReadyAsync(CancellationToken)"/> or <see cref="WaitReadyAsync(TimeSpan)"/> to wait for the bot to be ready.
        /// </summary>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public async Task StartAsync()
        {
            var gatewayInfo = await new DiscordGatewayClient(HttpClient).GetGatewayBotInfoAsync();

            _gate = new(gatewayInfo.SessionInfo.MaxConcurrency, TimeSpan.FromMicroseconds(gatewayInfo.SessionInfo.ResetAfter));
            _totalShards = Math.Max(_config.TotalShards ?? gatewayInfo.Shards, 1);

            // Start event processor pool
            _eventProcessorPool.Start();

            // Start shards asynchronously without blocking
            await InitShards(new Uri(gatewayInfo.Url));
        }

        /// <summary>
        /// Stops the Discord client and closes all Gateway connections.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public async Task StopAsync()
        {
            if (_isShuttingDown)
                return;

            _isShuttingDown = true;

            // Stop event processor pool
            await _eventProcessorPool.StopAsync();

            await ClearShardsAsync();
            _isInitialized = false;

            // Signal shutdown completion
            _shutdownEvent.Set();
        }

        private async Task InitShards(Uri gatewayUri)
        {
            await ClearShardsAsync();
            _shards.Clear();

            if (_gate == null)
                throw new InvalidOperationException("IdentifyGate must be initialized before creating shards.");

            for (int i = 0; i < _totalShards; i++)
            {
                var shard = new Shard(i, _config, _gate, gatewayUri);
                _shards.Add(shard);
                shard.OnResume += Shard_OnResume;
                shard.OnReady += Shard_OnReady;
                shard.ConnectionLost += Shard_ConnectionLost;
                shard.OnReceiveMessage += Shard_OnReceiveMessage;
                await shard.StartAsync();
            }
        }

        private async Task ClearShardsAsync()
        {
            if (_shards.Count == 0)
                return;

            try { _gate?.Dispose(); } catch { }

            for (int i = _shards.Count - 1; i >= 0; i--)
            {
                var shard = _shards[i];
                shard.OnResume -= Shard_OnResume;
                shard.OnReady -= Shard_OnReady;
                shard.ConnectionLost -= Shard_ConnectionLost;
                shard.OnReceiveMessage -= Shard_OnReceiveMessage;
                await shard.StopAsync();
                _shards.RemoveAt(i);
            }
        }

        /// <summary>
        /// Handles messages received from the Gateway and enqueues them for processing.
        /// </summary>
        private async Task Shard_OnReceiveMessage(Shard sender, ReceivedGatewayMessage arg)
        {
            if (arg.Opcode != OpCodes.Dispatch || string.IsNullOrEmpty(arg.EventType))
                return;

            Logger.Log(LogLevel.Trace, $"Received {arg.EventType} event from shard {sender.ShardId}");

            await _eventProcessorPool.EnqueueAsync(arg);
        }

        private Task Shard_ConnectionLost(Shard sender, Exception arg)
        {
            OnConnectionLost?.Invoke(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        private Task Shard_OnResume(Shard sender)
        {
            if (IsReady && OnReady != null)
                OnReady(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        private async Task Shard_OnReady(Shard sender, Gateway.Payloads.ReadyPayload arg)
        {
            if (string.IsNullOrEmpty(BotUser?.Id))
                BotUser = arg.User;

            // Store application ID from ready payload
            if (ApplicationId == null)
                ApplicationId = Snowflake.Parse(arg.Application.Id);

            Logger.Log(LogLevel.Information, $"Shard {sender.ShardId} of {BotUser.Username} is ready.");

            // Initialize pending guilds list from Ready payload (only once, on shard 0)
            if (IsReady && sender.ShardId == 0 && !_isInitialized)
            {
                _isInitialized = true;

                // Initialize pending guilds from Ready payload
                var guildIds = arg.Guilds
                    .Where(g => !string.IsNullOrEmpty(g.Id))
                    .Select(g => Snowflake.TryParse(g.Id, out var id) ? id : default);

                Guilds.InitializePendingGuilds(guildIds);
            }

            if (IsReady && OnReady != null)
                OnReady(this, EventArgs.Empty);

            if (IsReady)
                _readyEvent.Set();
        }

        /// <summary>
        /// Waits for the bot to be ready (all shards connected and ready).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the wait operation.</param>
        /// <returns>A task that completes when the bot is ready.</returns>
        public async Task WaitReadyAsync(CancellationToken cancellationToken = default)
        {
            if (IsReady)
                return;

            await Task.Run(() => _readyEvent.Wait(cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Waits for the bot to be ready (all shards connected and ready) with a timeout.
        /// </summary>
        /// <param name="timeout">Maximum time to wait for the bot to be ready.</param>
        /// <returns>A task that completes when the bot is ready or times out.</returns>
        /// <exception cref="TimeoutException">Thrown when the timeout is reached before the bot is ready.</exception>
        public async Task WaitReadyAsync(TimeSpan timeout)
        {
            if (IsReady)
                return;

            using var cts = new CancellationTokenSource(timeout);
            try
            {
                await Task.Run(() => _readyEvent.Wait(cts.Token), cts.Token);
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"The bot did not become ready within the specified timeout of {timeout.TotalSeconds} seconds.");
            }
        }

        /// <summary>
        /// Waits for the bot to shutdown.
        /// </summary>
        /// <param name="ct">Cancellation token to cancel the wait operation.</param>
        /// <returns>A task that completes when the bot is shutdown.</returns>
        public async Task WaitShutdownAsync(CancellationToken ct = default)
        {
            if (_isShuttingDown && _shutdownEvent.IsSet)
                return;

            await Task.Run(() => _shutdownEvent.Wait(ct), ct);
        }

        /// <summary>
        /// Waits for the bot to shutdown with a timeout.
        /// </summary>
        /// <param name="timeout">Maximum time to wait for the bot to shutdown.</param>
        /// <returns>A task that completes when the bot is shutdown or times out.</returns>
        /// <exception cref="TimeoutException">Thrown when the timeout is reached before the bot shuts down.</exception>
        public async Task WaitShutdownAsync(TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            try
            {
                await Task.Run(() => _shutdownEvent.Wait(cts.Token), cts.Token);
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"The bot did not shutdown within the specified timeout of {timeout.TotalSeconds} seconds.");
            }
        }

        internal int GetGuidShard(ulong guildId)
        {
            return (int)((guildId >> 22) % (ulong)TotalShards);
        }

        internal Shard GetShard(int shardId)
        {
            if (shardId < 0 || shardId >= TotalShards)
                throw new ArgumentOutOfRangeException(nameof(shardId), "Shard ID is out of range.");

            return _shards[shardId];
        }

        public IRestAction<TChannel?> GetChannel<TChannel>(Snowflake channelId) where TChannel : IChannel
        {
            return RestAction<TChannel?>.Create(async cancellationToken =>
            {
                var channel = await GetChannel(channelId).ExecuteAsync(cancellationToken);
                if (channel is not TChannel tChannel)
                    throw new InvalidCastException($"Channel '{channelId}' is not a '{typeof(TChannel).Name}'.");

                return tChannel;
            });
        }

        public IRestAction<IChannel?> GetChannel(Snowflake channelId)
        {
            if (channelId == default)
                throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

            return RestAction<IChannel?>.Create(async cancellationToken =>
            {
                var channel = await ChannelClient.GetAsync(channelId, cancellationToken);
                if (channel == null)
                    return null;

                // Get the guild if the channel belongs to a guild
                IGuild? guild = null;
                if (channel.GuildId.HasValue && !channel.GuildId.Value.Empty)
                    guild = await Guilds.GetAsync(channel.GuildId.Value, cancellationToken);

                return Wrappers.Channels.ChannelWrapper.ToSpecificType(this, channel, guild);
            });
        }

        public IRestAction<TimeSpan> Ping()
        {
            return RestAction<TimeSpan>.Create(async cancellationToken =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    // Use a simple endpoint to measure latency
                    await HttpClient.SendAsync("gateway", HttpMethod.Get, cancellationToken);
                }
                finally
                {
                    stopwatch.Stop();
                }
                return TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            });
        }

        /// <inheritdoc />
        public IRestAction<IDmChannel> OpenDm(Snowflake userId)
        {
            if (userId == default)
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            return RestAction<IDmChannel>.Create(async cancellationToken =>
            {
                var user = await Users.Get(userId).ExecuteAsync(cancellationToken);
                return user == null
                    ? throw new InvalidOperationException("User not found")
                    : await DmRepository.OpenDm(userId).ExecuteAsync(cancellationToken);
            });
        }

        /// <inheritdoc />
        public IRestAction<IUser?> GetUser(Snowflake userId)
        {
            return Users.Get(userId);
        }

        /// <inheritdoc />
        public IUpdatePresenceAction UpdatePresence()
        {
            return new UpdatePresenceAction(this);
        }
    }
}