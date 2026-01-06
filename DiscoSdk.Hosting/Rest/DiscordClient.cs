using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Payloads.Models;
using System.Threading;

namespace DiscoSdk.Hosting.Rest
{
    public class DiscordClient(DiscordClientConfig config)
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        public event EventHandler? OnReady;
        public event EventHandler? OnConnectionLost;

        private int _totalShards = 0;
        public int TotalShards => _totalShards;
        private readonly List<Shard> _shards = [];
        private IdentifyGate? _gate;
        private readonly IDiscordRestClientBase _client = new DiscordRestClientBase(config.Token, new Uri("https://discord.com/api/v10"));

        public bool IsReady => _shards.All(s => s.Status == ShardStatus.Ready);

        public ICurrentUser User { get; private set; } = new ReadyUser();

        public async Task StartAsync()
        {
            var gatewayInfo = await new DiscordRestClient(_client).GetGatewayBotInfoAsync();

            _gate = new(gatewayInfo.SessionInfo.MaxConcurrency, TimeSpan.FromMicroseconds(gatewayInfo.SessionInfo.ResetAfter));
            _totalShards = Math.Max(config.TotalShards ?? gatewayInfo.Shards, 1);

            await InitShards(new Uri(gatewayInfo.Url));
            await _semaphore.WaitAsync();
        }

        public async Task StopAsync()
        {
            _semaphore.Release();
            await ClearShardsAsync();
        }

        private async Task InitShards(Uri gatewayUri)
        {
            await ClearShardsAsync();
            _shards.Clear();

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

        private Task Shard_OnReceiveMessage(Shard sender, ReceivedGatewayMessage arg)
        {
            return Task.CompletedTask;
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

        private Task Shard_OnReady(Shard sender, Gateway.Payloads.ReadyPayload arg)
        {
            if (string.IsNullOrEmpty(User?.Id))
                User = arg.User;

            if (IsReady && OnReady != null)
                OnReady(this, EventArgs.Empty);

            Console.WriteLine($"Shard {sender.ShardId} of {User.Username} is ready.");

            return Task.CompletedTask;
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
    }
}