using DiscoSdk.Events;
using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Hosting.Gateway.Payloads;
using DiscoSdk.Hosting.Gateway.Payloads.Models;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Managers;
using DiscoSdk.Hosting.Observability;
using DiscoSdk.Hosting.Repositories;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Surfaces;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Channels;
using DiscoSdk.Modules;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiscoSdk.Hosting
{
    /// <summary>
    /// Main client for connecting to and managing Discord Gateway connections.
    /// </summary>
    public class DiscordClient : IDiscordClient, IShardEventListener
    {
        // RunContinuationsAsynchronously matters: TrySetResult runs from the receive loop / shutdown
        // path; without it, awaiters resume inline on the producer thread, risking reentrancy.
        private readonly TaskCompletionSource _shutdownTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _readyTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal IReadOnlyList<IDiscoModule> Modules { get; private set; } = [];
        public IDiscordRestClient HttpClient { get; }
        internal ChannelManager Channels { get; }
        public GuildManager Guilds { get; }

        public event Func<IDiscordClient, ICommandUpdateSession, Task>? CommandsUpdateWindowOpened;
        public event EventHandler<UnhandledErrorEventArgs>? UnhandledError;
        public event GatewayDisconnectedEventHandler? GatewayDisconnected;
        public event GatewayReconnectingEventHandler? GatewayReconnecting;

        private EventProcessorPool<ReceivedGatewayMessage> _eventProcessorPool = null!;
        private readonly DiscordEventDispatcher _eventDispatcher;
        private readonly DiscordClientConfig _config;
        private readonly ShardPool _shardPool;
        // Set once via Interlocked when any shard exits the run loop with an unrecoverable
        // exception. The Wait* methods consult ThrowIfFatal() and rethrow wrapped in DiscordFatalException.
        private Exception? _fatalException;

        /// <summary>
        /// Gets the gateway intents configured for this client.
        /// </summary>
        public DiscordIntent Intents => _config.Intents;
        // 0/1 instead of bool so the transition can be done atomically with Interlocked.Exchange.
        private int _isInitialized;
        private int _isShuttingDown;

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
        public int TotalShards => _shardPool.TotalShards;

        internal InteractionClient InteractionClient { get; }
        internal MessageClient MessageClient { get; }
        internal ChannelClient ChannelClient { get; }
        internal InviteClient InviteClient { get; }
        internal RoleClient RoleClient { get; }
        internal GuildClient GuildClient { get; }
        internal AutoModerationClient AutoModerationClient { get; }
        internal ApplicationClient ApplicationClient { get; }
        internal GuildTemplateClient GuildTemplateClient { get; }
        internal UserClient UserClient { get; }
        internal WebhookClient WebhookClient { get; }
        internal StageInstanceClient StageInstanceClient { get; }
        internal GuildScheduledEventClient GuildScheduledEventClient { get; }
        internal StickerClient StickerClient { get; }
        internal OAuth2Client OAuth2Client { get; }
        internal ApplicationCommandClient ApplicationCommandClient { get; }
        internal UserRepository Users { get; }

        /// <inheritdoc />
        public IOAuth2 OAuth2 { get; }
        /// <inheritdoc />
        public IMonetization Monetization { get; }
        /// <inheritdoc />
        public IMe Me { get; }
        /// <inheritdoc />
        public IWebhooks Webhooks { get; }
        /// <inheritdoc />
        public IApplicationEmojis ApplicationEmojis { get; }
        internal DmChannelRepository DmRepository { get; }

        /// <summary>
        /// Gets or sets the application ID of the bot.
        /// </summary>
        public Snowflake? ApplicationId { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether all shards are ready.
        /// </summary>
        public bool IsReady => _shardPool.Shards.All(s => s.Status == ShardStatus.Ready);

        /// <summary>
        /// Gets a value indicating whether the bot has fully initialized (all guilds have been loaded).
        /// </summary>
        public bool IsFullyInitialized => Guilds.IsFullyInitialized;

        /// <summary>
        /// Gets the current authenticated user.
        /// </summary>
        public ICurrentUser BotUser { get; private set; } = new ReadyUser();

        public IServiceProvider Services { get; }

        internal DiscordClient(IServiceProvider services,
            DiscordClientConfig config,
            TimeProvider timeProvider,
            IGatewaySocketFactory socketFactory,
            JsonSerializerOptions serializerOptions,
            ILogger logger,
            IDiscordRestClient httpClient,
            IObjectConverter objectConverter)
        {
            _config = config;
            Services = services;
            _shardPool = new ShardPool(this, config, socketFactory, timeProvider);
            SerializerOptions = serializerOptions;
            Logger = logger;
            _eventDispatcher = new DiscordEventDispatcher(this);
            HttpClient = httpClient;
            InteractionClient = new InteractionClient(this);
            MessageClient = new MessageClient(HttpClient);
            ChannelClient = new ChannelClient(HttpClient, MessageClient);
            InviteClient = new InviteClient(HttpClient);
            RoleClient = new RoleClient(HttpClient);
            GuildClient = new GuildClient(HttpClient);
            AutoModerationClient = new AutoModerationClient(HttpClient);
            ApplicationClient = new ApplicationClient(HttpClient);
            GuildTemplateClient = new GuildTemplateClient(HttpClient);
            UserClient = new UserClient(HttpClient);
            WebhookClient = new WebhookClient(HttpClient);
            StageInstanceClient = new StageInstanceClient(HttpClient);
            GuildScheduledEventClient = new GuildScheduledEventClient(HttpClient);
            StickerClient = new StickerClient(HttpClient);
            OAuth2Client = new OAuth2Client(HttpClient);
            ApplicationCommandClient = new ApplicationCommandClient(HttpClient);
            OAuth2 = new OAuth2Surface(this);
            Monetization = new MonetizationSurface(this);
            Me = new MeSurface(this);
            Webhooks = new WebhooksSurface(this);
            ApplicationEmojis = new ApplicationEmojisSurface(this);
            Users = new UserRepository(this);
            Guilds = new GuildManager(this, Logger);
            Channels = new ChannelManager(this);
            DmRepository = new DmChannelRepository(this);
            ObjectConverter = objectConverter;
        }

        internal void InternalInit(IReadOnlyList<IDiscoModule> modules, IReadOnlyList<IDiscordEventHandler> eventHandlers)
        {
            Modules = modules;
            var maxConcurrency = _config.EventProcessorMaxConcurrency > 0
                ? _config.EventProcessorMaxConcurrency
                : Environment.ProcessorCount * 2;

            _eventDispatcher
                .AddAll(modules.OfType<IDiscordEventHandler>())
                .AddAll(eventHandlers);

            var queueCapacity = Math.Max(1, _config.EventProcessorQueueCapacity);
            _eventProcessorPool = new EventProcessorPool<ReceivedGatewayMessage>(maxConcurrency, _eventDispatcher.ProcessEventAsync, Logger, queueCapacity);
        }

        /// <summary>
        /// Starts the Discord client and establishes connections to the Gateway.
        /// This method returns immediately after starting the connection process.
        /// Use <see cref="WaitReadyAsync(CancellationToken)"/> or <see cref="WaitReadyAsync(TimeSpan)"/> to wait for the bot to be ready.
        /// </summary>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public async Task StartAsync()
        {
            LogPrivilegedIntentReminder();

            var gatewayInfo = await new DiscordGatewayClient(HttpClient).GetGatewayBotInfoAsync();

            foreach (var module in Modules.OfType<ILifetimeDiscoModule>())
            {
                try { await module.OnPreInitializeAsync(this); } catch { }
                if (module is IDiscordEventHandler handler)
                    _eventDispatcher.Add(handler);
            }

            // Start event processor pool
            _eventProcessorPool.Start();
            _shardPool.SetGateway(gatewayInfo);
            await _shardPool.InitShardsAsync();
        }

        /// <summary>
        /// Emits a one-shot Information log naming every privileged intent the bot requested
        /// (<see cref="DiscordIntent.GuildMembers"/>, <see cref="DiscordIntent.GuildPresences"/>,
        /// <see cref="DiscordIntent.MessageContent"/>) so the operator has a single grep target
        /// when chasing a Discord close code 4014. Sending a privileged bit in <c>IDENTIFY</c>
        /// without the matching Developer Portal flag is the cause; surfacing the requested set
        /// here turns "shard refuses to identify" into a one-line correlation, instead of an
        /// opaque connection loop.
        /// </summary>
        internal void LogPrivilegedIntentReminder()
        {
            const DiscordIntent privilegedMask =
                DiscordIntent.GuildMembers
                | DiscordIntent.GuildPresences
                | DiscordIntent.MessageContent;

            var privileged = _config.Intents & privilegedMask;
            if (privileged == DiscordIntent.None)
                return;

            Logger.Log(
                LogLevel.Information,
                "Privileged gateway intents requested: {Privileged}. Each must also be enabled in the Discord Developer Portal — sending a privileged bit in IDENTIFY without the portal flag earns close code 4014 (Disallowed Intents) and refuses the session.",
                privileged);
        }

        /// <summary>
        /// Stops the Discord client and closes all Gateway connections.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public async Task StopAsync()
        {
            // Atomic transition. The first caller does the work; a concurrent / re-entrant second
            // caller awaits the shutdown TCS instead of returning prematurely (the previous
            // `if (_isShuttingDown) return;` lied — the second caller's `await StopAsync()` resolved
            // before the first caller had actually torn anything down).
            if (Interlocked.Exchange(ref _isShuttingDown, 1) == 1)
            {
                await _shutdownTcs.Task;
                return;
            }

            try
            {
                foreach (var item in Modules.OfType<ILifetimeDiscoModule>())
                    try { await item.OnShutdownAsync(this); } catch { }

                await _eventProcessorPool.StopAsync();
                await _shardPool.ClearShardsAsync();
                _shardPool.Dispose();
                Interlocked.Exchange(ref _isInitialized, 0);
            }
            finally
            {
                // Always wake awaiters, even if shutdown work threw — they'd hang forever otherwise.
                _shutdownTcs.TrySetResult();
            }
        }

        /// <summary>
        /// Waits for the bot to be ready (all shards connected and ready).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the wait operation.</param>
        public async Task WaitReadyAsync(CancellationToken cancellationToken = default)
        {
            if (IsReady)
            {
                ThrowIfFatal();
                return;
            }

            await _readyTcs.Task.WaitAsync(cancellationToken);
            ThrowIfFatal();
        }

        /// <summary>
        /// Waits for the bot to be ready (all shards connected and ready) with a timeout.
        /// </summary>
        /// <exception cref="TimeoutException">Thrown when the timeout is reached before the bot is ready.</exception>
        public async Task WaitReadyAsync(TimeSpan timeout)
        {
            if (IsReady)
            {
                ThrowIfFatal();
                return;
            }

            try
            {
                await _readyTcs.Task.WaitAsync(timeout);
            }
            catch (TimeoutException)
            {
                ThrowIfFatal();
                throw new TimeoutException($"The bot did not become ready within the specified timeout of {timeout.TotalSeconds} seconds.");
            }

            ThrowIfFatal();
        }

        /// <summary>
        /// Waits for the bot to shutdown.
        /// </summary>
        /// <param name="ct">Cancellation token to cancel the wait operation.</param>
        public async Task WaitShutdownAsync(CancellationToken ct = default)
        {
            if (Volatile.Read(ref _isShuttingDown) == 1 && _shutdownTcs.Task.IsCompleted)
            {
                ThrowIfFatal();
                return;
            }

            await _shutdownTcs.Task.WaitAsync(ct);
            ThrowIfFatal();
        }

        /// <summary>
        /// Waits for the bot to shutdown with a timeout.
        /// </summary>
        /// <exception cref="TimeoutException">Thrown when the timeout is reached before shutdown.</exception>
        public async Task WaitShutdownAsync(TimeSpan timeout)
        {
            try
            {
                await _shutdownTcs.Task.WaitAsync(timeout);
            }
            catch (TimeoutException)
            {
                ThrowIfFatal();
                throw new TimeoutException($"The bot did not shutdown within the specified timeout of {timeout.TotalSeconds} seconds.");
            }

            ThrowIfFatal();
        }

        internal int GetGuidShard(ulong guildId)
        {
            return (int)((guildId >> 22) % (ulong)TotalShards);
        }

        internal Shard GetShard(int shardId)
        {
            if (shardId < 0 || shardId >= TotalShards)
                throw new ArgumentOutOfRangeException(nameof(shardId), "Shard ID is out of range.");

            return _shardPool.Shards[shardId];
        }

        /// <inheritdoc />
        public IReadOnlyList<IShard> Shards => _shardPool.Shards;

        /// <inheritdoc />
        public async Task ReconnectAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Soft reconnect: tear down + reopen every shard's transport. Does NOT re-run module
            // OnPreInitialize / OnGatewayReady hooks, refetch gateway info, or repeat slash command
            // registration — _isInitialized stays at 1, the next OnReadyAsync sees it and skips the
            // first-ready block.
            await _shardPool.ClearShardsAsync();
            await _shardPool.InitShardsAsync();
        }

        /// <summary>
        /// Picks the shard responsible for a guild using Discord's sharding formula
        /// <c>(guild_id &gt;&gt; 22) % num_shards</c>. Used by the gateway commands that target a
        /// specific guild (Request Guild Members, Update Voice State, etc.).
        /// </summary>
        internal Shard GetShardForGuild(Snowflake guildId)
        {
            var shardId = (int)((guildId.Value >> 22) % (ulong)Math.Max(TotalShards, 1));
            return _shardPool.Shards[shardId];
        }

        /// <summary>
        /// Coordinator that correlates inbound <c>GUILD_MEMBERS_CHUNK</c> events with in-flight
        /// <c>Request Guild Members</c> (op 8) calls by nonce.
        /// </summary>
        internal MemberChunkCoordinator MemberChunkCoordinator { get; } = new();

        /// <summary>
        /// Test-only seam — seeds the shard list so <see cref="GetShardForGuild"/> works without
        /// running the full <c>InitShardsAsync</c> flow.
        /// </summary>
        internal void SeedShardsForTests(int totalShards = 1) => _shardPool.SeedShardsForTests(totalShards);

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
                    await HttpClient.SendAsync(new DiscordRoute("gateway"), HttpMethod.Get, cancellationToken);
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

        internal Snowflake RequireApplicationId()
            => ApplicationId ?? throw new InvalidOperationException("The application ID is not available yet — wait until the client is ready.");

        /// <inheritdoc />
        public IRestAction<IApplication> GetApplication()
            => RestAction<IApplication>.Create(async ct => new ApplicationWrapper(this, await ApplicationClient.GetCurrentApplicationAsync(ct)));

        /// <inheritdoc />
        public IRestAction<ISticker> GetSticker(Snowflake stickerId)
            => RestAction<ISticker>.Create(async ct =>
                new StickerWrapper(this, await StickerClient.GetStickerAsync(stickerId, ct)));

        /// <inheritdoc />
        public IRestAction<IReadOnlyList<IStickerPack>> GetStickerPacks()
            => RestAction<IReadOnlyList<IStickerPack>>.Create(async ct =>
            {
                var envelope = await StickerClient.ListStickerPacksAsync(ct);
                return envelope.StickerPacks.Select(p => (IStickerPack)new StickerPackWrapper(this, p)).ToList().AsReadOnly();
            });

        /// <inheritdoc />
        public IRestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>> GetRoleConnectionMetadata()
            => RestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>>.Create(async ct => await ApplicationClient.GetRoleConnectionMetadataAsync(RequireApplicationId(), ct));

        /// <inheritdoc />
        public IRestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>> UpdateRoleConnectionMetadata(IEnumerable<ApplicationRoleConnectionMetadata> records)
        {
            ArgumentNullException.ThrowIfNull(records);
            return RestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>>.Create(async ct => await ApplicationClient.UpdateRoleConnectionMetadataAsync(RequireApplicationId(), records, ct));
        }

        /// <inheritdoc />
        public IGetInviteAction GetInvite(string code) => new GetInviteAction(this, code);

        /// <inheritdoc />
        public IRestAction<ActivityInstance?> GetActivityInstance(string instanceId)
            => RestAction<ActivityInstance?>.Create(ct =>
                ApplicationClient.GetActivityInstanceAsync(RequireApplicationId(), instanceId, ct));

        /// <inheritdoc />
        public ICreateGroupDmAction CreateGroupDm()
            => new CreateGroupDmAction(this);

        /// <inheritdoc />
        public IReadOnlyList<IDmChannel> OpenedDms => DmRepository.GetAll();

        async Task IShardEventListener.OnReceiveMessageAsync(Shard shard, ReceivedGatewayMessage message)
        {
            if (message.Opcode != OpCodes.Dispatch || string.IsNullOrEmpty(message.EventType))
                return;

            DiscoSdkDiagnostics.GatewayEventsReceived.Add(
                1,
                new KeyValuePair<string, object?>(DiagnosticTags.ShardId, shard.Id),
                new KeyValuePair<string, object?>(DiagnosticTags.EventType, message.EventType));

            Logger.Log(LogLevel.Trace, "Received {EventType} event from shard {ShardId}", message.EventType, shard.Id);

            await _eventProcessorPool.EnqueueAsync(message);
        }

        async Task IShardEventListener.OnReadyAsync(Shard shard, ReadyPayload payload)
        {
            if (string.IsNullOrEmpty(BotUser?.Id))
                BotUser = payload.User;

            // Store application ID from ready payload
            if (ApplicationId == null)
                ApplicationId = Snowflake.Parse(payload.Application.Id);

            Logger.Log(LogLevel.Information, "Shard {ShardId} of {BotUsername} is ready.", shard.Id, BotUser.Username);

            // First-fully-ready work — runs exactly once across the client's lifetime. Subsequent
            // READY frames (a shard reconnecting via auto-retry, a manual ReconnectAsync, RESUMED
            // failures escalating to fresh identify) skip this block: command registration,
            // lifetime module hooks, and pending-guild seeding are already done.
            if (IsReady && Interlocked.Exchange(ref _isInitialized, 1) == 0)
            {
                if (shard.Id == 0)
                {
                    var guildIds = payload.Guilds
                        .Where(g => !string.IsNullOrEmpty(g.Id))
                        .Select(g => Snowflake.TryParse(g.Id, out var id) ? id : default);

                    Guilds.InitializePendingGuilds(guildIds);
                }

                foreach (var item in Modules.OfType<ILifetimeDiscoModule>())
                    try { await item.OnGatewayReadyAsync(this); } catch { }

                await InitSlashCommandsAsync();
            }

            // One-shot — matches the docstring "Event raised when all shards are ready". Subsequent
            // shard reconnects do not refire OnReady; subscribers tracking individual shard liveness
            // listen to GatewayDisconnected + IShard.IsReady instead.
            if (IsReady && !_readyTcs.Task.IsCompleted)
            {
                OnReady?.Invoke(this, EventArgs.Empty);
                _readyTcs.TrySetResult();
            }
        }

        private async Task InitSlashCommandsAsync()
        {
            // ApplicationId is now populated (gateway READY). The factory is a DI singleton —
            // resolve it and build a session shared between the auto-register module and the
            // user event handler; both accumulate into the same scopes and we commit once.
            var factory = Services.GetRequiredService<CommandUpdateFactory>();
            var session = new CommandUpdateSession(factory);

            foreach (var module in Modules.OfType<ICommandsUpdateWindowModule>())
                await module.OnCommandsUpdateWindowOpenedAsync(this, session);

            if (CommandsUpdateWindowOpened is { } evt)
            {
                foreach (var handler in evt.GetInvocationList().Cast<Func<IDiscordClient, ICommandUpdateSession, Task>>())
                    await handler(this, session);
            }

            await session.ApplyAllAsync();
        }

        Task IShardEventListener.OnResumeAsync(Shard shard)
        {
            if (IsReady)
            {
                if (_readyTcs.Task.IsCompleted == false)
                {
                    OnReady?.Invoke(this, EventArgs.Empty);
                    _readyTcs.TrySetResult();
                }
            }

            return Task.CompletedTask;
        }

        async Task IShardEventListener.OnConnectionLostAsync(Shard shard, Exception exception)
        {
            OnConnectionLost?.Invoke(this, EventArgs.Empty);

            if (GatewayDisconnected is { } evt)
            {
                // WillReconnect reflects what the shard's catch path will actually do: non-transport
                // exceptions get routed to OnFatalAsync (no reconnect), and AutoReconnect=false also
                // skips the retry. Classification lives in GatewayExceptions so Shard and DiscordClient
                // never drift.
                var willReconnect = _config.AutoReconnect && GatewayExceptions.IsRecoverableTransport(exception);
                var args = new GatewayDisconnectedEventArgs(shard, exception, willReconnect);
                foreach (var handler in evt.GetInvocationList().Cast<GatewayDisconnectedEventHandler>())
                    await handler(args);
            }
        }

        void IShardEventListener.OnUnhandledError(Exception exception)
        {
            Logger.Log(LogLevel.Error, exception, "Unhandled shard error");

            UnhandledError?.Invoke(this, new UnhandledErrorEventArgs(exception));
        }

        Task IShardEventListener.OnFatalAsync(Shard shard, Exception exception)
        {
            Logger.Log(LogLevel.Critical, exception, "Shard {ShardId} hit a fatal error — terminating client.", shard.Id);
            MarkFatal(exception);
            return Task.CompletedTask;
        }

        async Task IShardEventListener.OnReconnectingAsync(Shard shard, int attempt, TimeSpan delay, bool isResume)
        {
            Logger.Log(LogLevel.Information, "Shard {ShardId} reconnect attempt {Attempt} in {DelaySeconds:F1}s (resume={IsResume}).",
                shard.Id, attempt, delay.TotalSeconds, isResume);

            if (GatewayReconnecting is { } evt)
            {
                var args = new GatewayReconnectingEventArgs(shard, attempt, delay, isResume);
                foreach (var handler in evt.GetInvocationList().Cast<GatewayReconnectingEventHandler>())
                    await handler(args);
            }
        }

        /// <summary>
        /// Capture the first fatal exception and wake every Wait* awaiter. The TCS only carry "done"
        /// state — the actual rethrow happens in <see cref="ThrowIfFatal"/> after the wait, so the
        /// fatal flow stays visible at every Wait* call site.
        /// </summary>
        private void MarkFatal(Exception exception)
        {
            // First fatal wins; subsequent shards reporting the same outage do not overwrite the cause.
            if (Interlocked.CompareExchange(ref _fatalException, exception, null) is not null)
                return;

            _readyTcs.TrySetResult();
            _shutdownTcs.TrySetResult();
        }

        private void ThrowIfFatal()
        {
            if (_fatalException is { } ex)
                throw new DiscordFatalException(GatewayMessages.DiscordClientTerminated, ex);
        }
    }
}