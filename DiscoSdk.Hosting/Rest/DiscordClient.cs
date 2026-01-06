using DiscoSdk.Hosting.Events;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Payloads.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest
{
    /// <summary>
    /// Main client for connecting to and managing Discord Gateway connections.
    /// </summary>
    public class DiscordClient(DiscordClientConfig config)
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        /// <summary>
        /// Event raised when all shards are ready and the client is fully connected.
        /// </summary>
        public event EventHandler? OnReady;

        /// <summary>
        /// Event raised when the connection to Discord is lost.
        /// </summary>
        public event EventHandler? OnConnectionLost;

        private int _totalShards = 0;

        /// <summary>
        /// Gets the total number of shards being used.
        /// </summary>
        public int TotalShards => _totalShards;

        private readonly List<Shard> _shards = [];
        private IdentifyGate? _gate;
        private readonly IDiscordRestClientBase _client = new DiscordRestClientBase(config.Token, new Uri("https://discord.com/api/v10"));
        private bool _commandsRegistered = false;
        private readonly HashSet<string> _knownGuildIds = [];

        /// <summary>
        /// Gets the event dispatcher for handling Gateway events.
        /// </summary>
        public DiscordEventDispatcher EventDispatcher { get; } = new DiscordEventDispatcher();

        /// <summary>
        /// Gets the interaction client for responding to interactions.
        /// </summary>
        public InteractionClient InteractionClient { get; } = new InteractionClient(new DiscordRestClientBase(config.Token, new Uri("https://discord.com/api/v10")));

        /// <summary>
        /// Gets the application command client for registering commands.
        /// </summary>
        public ApplicationCommandClient ApplicationCommandClient { get; } = new ApplicationCommandClient(new DiscordRestClientBase(config.Token, new Uri("https://discord.com/api/v10")));

        /// <summary>
        /// Gets or sets the application ID of the bot.
        /// </summary>
        public string? ApplicationId { get; private set; }

        /// <summary>
        /// Gets or sets the list of commands to register globally when the bot starts.
        /// </summary>
        public List<Models.ApplicationCommand> GlobalCommands { get; } = [];

        /// <summary>
        /// Gets or sets the dictionary of guild-specific commands to register when the bot starts.
        /// Key is the guild ID, value is the list of commands for that guild.
        /// </summary>
        public Dictionary<string, List<Models.ApplicationCommand>> GuildCommands { get; } = [];

        /// <summary>
        /// Gets the JSON serializer options used for deserializing Gateway events.
        /// </summary>
        private static JsonSerializerOptions JsonOptions => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Gets a value indicating whether all shards are ready.
        /// </summary>
        public bool IsReady => _shards.All(s => s.Status == ShardStatus.Ready);

        /// <summary>
        /// Gets the current authenticated user.
        /// </summary>
        public ICurrentUser User { get; private set; } = new ReadyUser();

        /// <summary>
        /// Starts the Discord client and establishes connections to the Gateway.
        /// </summary>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public async Task StartAsync()
        {
            var gatewayInfo = await new DiscordRestClient(_client).GetGatewayBotInfoAsync();

            _gate = new(gatewayInfo.SessionInfo.MaxConcurrency, TimeSpan.FromMicroseconds(gatewayInfo.SessionInfo.ResetAfter));
            _totalShards = Math.Max(config.TotalShards ?? gatewayInfo.Shards, 1);

            await InitShards(new Uri(gatewayInfo.Url));
            await _semaphore.WaitAsync();
        }

        /// <summary>
        /// Stops the Discord client and closes all Gateway connections.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public async Task StopAsync()
        {
            _semaphore.Release();
            await ClearShardsAsync();
        }

        private async Task InitShards(Uri gatewayUri)
        {
            await ClearShardsAsync();
            _shards.Clear();

            if (_gate == null)
                throw new InvalidOperationException("IdentifyGate must be initialized before creating shards.");

            for (int i = 0; i < _totalShards; i++)
            {
                var shard = new Shard(i, config.Token, config.Intents, _gate, gatewayUri);
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
        /// Handles messages received from the Gateway and dispatches them to registered event handlers.
        /// </summary>
        private async Task Shard_OnReceiveMessage(Shard sender, ReceivedGatewayMessage arg)
        {
            if (arg.Opcode != OpCodes.Dispatch || string.IsNullOrEmpty(arg.EventType))
                return;

            // Debug: Log received events (optional, can be removed later)
            if (arg.EventType == "MESSAGE_CREATE")
            {
                Console.WriteLine($"[DEBUG] Received MESSAGE_CREATE event from shard {sender.ShardId}");
            }
            else if (arg.EventType == "INTERACTION_CREATE")
            {
                Console.WriteLine($"[DEBUG] Received INTERACTION_CREATE event from shard {sender.ShardId}");
            }

            await EventDispatcher.ProcessEventAsync(this, arg.EventType, arg.Payload, JsonOptions);
        }

        private Task Shard_ConnectionLost(Shard sender, Exception arg)
        {
            if (IsReady && OnReady != null)
                OnReady(this, EventArgs.Empty);

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
            if (string.IsNullOrEmpty(User?.Id))
                User = arg.User;

            // Store application ID from ready payload
            if (string.IsNullOrEmpty(ApplicationId))
                ApplicationId = arg.Application.Id;

            Console.WriteLine($"Shard {sender.ShardId} of {User.Username} is ready.");

            // Register commands when all shards are ready (only once, on shard 0)
            if (IsReady && sender.ShardId == 0 && !_commandsRegistered)
            {
                _commandsRegistered = true;
                await RegisterCommandsAsync();
            }

            if (IsReady && OnReady != null)
                OnReady(this, EventArgs.Empty);
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

        /// <summary>
        /// Registers all configured application commands with Discord.
        /// This method is called automatically when all shards are ready.
        /// Commands not in the list will be removed (replaced by the provided list).
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task RegisterCommandsAsync()
        {
            if (string.IsNullOrEmpty(ApplicationId))
            {
                Console.WriteLine("[WARNING] Application ID not available. Commands will not be registered.");
                return;
            }

            try
            {
                // Always register global commands (even if empty list, this removes all existing commands)
                // PUT replaces ALL commands, so sending an empty list removes all commands
                Console.WriteLine($"[COMMANDS] Registering {GlobalCommands.Count} global command(s) (removing all others)...");
                var registered = await ApplicationCommandClient.RegisterGlobalCommandsAsync(ApplicationId, GlobalCommands);
                Console.WriteLine($"[COMMANDS] Successfully registered {registered.Count} global command(s). All other global commands were removed.");

                // Register guild-specific commands for each guild
                // PUT replaces ALL commands for that guild, so sending an empty list removes all commands for that guild
                foreach (var (guildId, commands) in GuildCommands)
                {
                    Console.WriteLine($"[COMMANDS] Registering {commands.Count} command(s) for guild {guildId} (removing all others for this guild)...");
                    var guildRegistered = await ApplicationCommandClient.RegisterGuildCommandsAsync(ApplicationId, guildId, commands);
                    Console.WriteLine($"[COMMANDS] Successfully registered {guildRegistered.Count} command(s) for guild {guildId}. All other commands for this guild were removed.");
                }

                // Note: Guilds not in GuildCommands dictionary will keep their existing commands
                // To remove commands from a guild, add it to GuildCommands with an empty list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to register commands: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}