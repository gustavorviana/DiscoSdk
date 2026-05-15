using DiscoSdk.Events;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Hosting.Gateway.Payloads;
using DiscoSdk.Hosting.Gateway.Payloads.Models;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Managers;
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
        private readonly ManualResetEventSlim _shutdownEvent = new(false);
        private readonly ManualResetEventSlim _readyEvent = new(false);
        internal IReadOnlyList<IDiscoModule> Modules { get; private set; } = [];
        public IDiscordRestClient HttpClient { get; }
        internal ChannelManager Channels { get; }
        public GuildManager Guilds { get; }

        public event Func<IDiscordClient, ICommandUpdateSession, Task>? CommandsUpdateWindowOpened;
        public event EventHandler<UnhandledErrorEventArgs>? UnhandledError;

        private EventProcessorPool<ReceivedGatewayMessage> _eventProcessorPool = null!;
        private readonly DiscordEventDispatcher _eventDispatcher;
        private readonly DiscordClientConfig _config;
        private readonly ShardPool _shardPool;

        /// <summary>
        /// Gets the gateway intents configured for this client.
        /// </summary>
        public DiscordIntent Intents => _config.Intents;
        private bool _isInitialized = false;
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
        /// Stops the Discord client and closes all Gateway connections.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public async Task StopAsync()
        {
            if (_isShuttingDown)
                return;

            _isShuttingDown = true;

            foreach (var item in Modules.OfType<ILifetimeDiscoModule>())
                try { await item.OnShutdownAsync(this); } catch { }

            // Stop event processor pool
            await _eventProcessorPool.StopAsync();
            await _shardPool.ClearShardsAsync();
            _isInitialized = false;

            // Signal shutdown completion
            _shutdownEvent.Set();
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

            return _shardPool.Shards[shardId];
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
        public IRestAction<IInvite?> GetInvite(string code, bool? withCounts = null, bool? withExpiration = null, Snowflake? guildScheduledEventId = null)
            => RestAction<IInvite?>.Create(async ct =>
            {
                var invite = await InviteClient.GetAsync(code, withCounts, withExpiration, guildScheduledEventId, ct);
                if (invite is null) return null;

                if (invite.Channel?.Id is { } chId)
                {
                    var channel = await GetChannel(chId).ExecuteAsync(ct);
                    if (channel is IGuildChannelBase guildChannel)
                        return new InviteWrapper(invite, guildChannel, this);
                }

                return null;
            });

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

            Logger.Log(LogLevel.Trace, "Received {EventType} event from shard {ShardId}", message.EventType, shard.ShardId);

            await _eventProcessorPool.EnqueueAsync(message);
        }

        async Task IShardEventListener.OnReadyAsync(Shard shard, ReadyPayload payload)
        {
            if (string.IsNullOrEmpty(BotUser?.Id))
                BotUser = payload.User;

            // Store application ID from ready payload
            if (ApplicationId == null)
                ApplicationId = Snowflake.Parse(payload.Application.Id);

            Logger.Log(LogLevel.Information, "Shard {ShardId} of {BotUsername} is ready.", shard.ShardId, BotUser.Username);

            // Initialize pending guilds list from Ready payload (only once, on shard 0)
            if (IsReady && shard.ShardId == 0 && !_isInitialized)
            {
                _isInitialized = true;

                // Initialize pending guilds from Ready payload
                var guildIds = payload.Guilds
                    .Where(g => !string.IsNullOrEmpty(g.Id))
                    .Select(g => Snowflake.TryParse(g.Id, out var id) ? id : default);

                Guilds.InitializePendingGuilds(guildIds);
            }

            foreach (var item in Modules.OfType<ILifetimeDiscoModule>())
                try { await item.OnGatewayReadyAsync(this); } catch { }

            await InitSlashCommandsAsync();

            if (IsReady && OnReady != null)
                OnReady(this, EventArgs.Empty);

            if (IsReady)
                _readyEvent.Set();
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
            if (IsReady && OnReady != null)
                OnReady(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        Task IShardEventListener.OnConnectionLostAsync(Shard shard, Exception exception)
        {
            OnConnectionLost?.Invoke(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        void IShardEventListener.OnUnhandledError(Exception exception)
        {
            Logger.Log(LogLevel.Error, exception, "Unhandled shard error");

            UnhandledError?.Invoke(this, new UnhandledErrorEventArgs(exception));
        }
    }
}