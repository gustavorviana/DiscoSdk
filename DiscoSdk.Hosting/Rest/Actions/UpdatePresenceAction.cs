using DiscoSdk.Hosting.Gateway.Payloads;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal class UpdatePresenceAction(DiscordClient client) : RestAction, IUpdatePresenceAction
{
    private OnlineStatus? _status;
    private Activity[]? _activities;
    private bool? _afk;
    private long? _since;

    public IUpdatePresenceAction SetStatus(OnlineStatus status)
    {
        _status = status;
        return this;
    }

    public IUpdatePresenceAction SetActivities(Activity[]? activities)
    {
        _activities = activities;
        return this;
    }

    public IUpdatePresenceAction AddActivity(Activity activity)
    {
        _activities ??= [];

        var list = _activities.ToList();
        list.Add(activity);
        _activities = [.. list];
        return this;
    }

    public IUpdatePresenceAction AddActivity(IActivity activity)
    {
        return AddActivity(activity.Build());
    }

    public IUpdatePresenceAction SetAfk(bool afk)
    {
        _afk = afk;
        return this;
    }

    public IUpdatePresenceAction SetSince(DateTimeOffset? since)
    {
        _since = since?.ToUnixTimeMilliseconds();
        return this;
    }

    public IUpdatePresenceAction ClearActivities()
    {
        _activities = [];
        return this;
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var presenceData = new PresenceUpdatePayload();

        if (_status.HasValue)
        {
            presenceData.Status = _status.Value switch
            {
                OnlineStatus.Online => "online",
                OnlineStatus.Idle => "idle",
                OnlineStatus.DoNotDisturb => "dnd",
                OnlineStatus.Invisible => "invisible",
                _ => "online"
            };
        }

        if (_activities != null)
        {
            presenceData.Activities = _activities.Select(SerializeActivity).ToArray();
        }

        if (_afk.HasValue)
        {
            presenceData.Afk = _afk.Value;
        }

        if (_since.HasValue)
        {
            presenceData.Since = _since.Value;
        }

        // Send presence update to all shards
        if (client.TotalShards > 0)
        {
            var tasks = new List<Task>();
            for (int i = 0; i < client.TotalShards; i++)
            {
                var shard = client.GetShard(i);
                tasks.Add(shard.SendAsync(Gateway.OpCodes.PresenceUpdate, presenceData, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
    }

    private static ActivityPayload SerializeActivity(Activity activity)
    {
        var payload = new ActivityPayload
        {
            Name = activity.Name,
            Type = (int)activity.Type,
            Url = activity.Url,
            CreatedAt = activity.CreatedAt,
            ApplicationId = activity.ApplicationId?.ToString(),
            Details = activity.Details,
            State = activity.State,
            Instance = activity.Instance,
            Flags = activity.Flags
        };

        if (activity.Timestamps != null)
        {
            var timestamps = new ActivityTimestampsPayload
            {
                Start = activity.Timestamps.Start,
                End = activity.Timestamps.End
            };
            if (timestamps.Start.HasValue || timestamps.End.HasValue)
                payload.Timestamps = timestamps;
        }

        if (activity.Emoji != null)
        {
            payload.Emoji = new ActivityEmojiPayload
            {
                Name = activity.Emoji.Name,
                Id = activity.Emoji.Id?.ToString(),
                Animated = activity.Emoji.Animated
            };
        }

        if (activity.Party != null)
        {
            var party = new ActivityPartyPayload
            {
                Id = activity.Party.Id,
                Size = activity.Party.Size
            };
            if (party.Id != null || party.Size != null)
                payload.Party = party;
        }

        if (activity.Assets != null)
        {
            var assets = new ActivityAssetsPayload
            {
                LargeImage = activity.Assets.LargeImage,
                LargeText = activity.Assets.LargeText,
                SmallImage = activity.Assets.SmallImage,
                SmallText = activity.Assets.SmallText
            };
            if (assets.LargeImage != null || assets.LargeText != null || 
                assets.SmallImage != null || assets.SmallText != null)
                payload.Assets = assets;
        }

        if (activity.Secrets != null)
        {
            var secrets = new ActivitySecretsPayload
            {
                Join = activity.Secrets.Join,
                Spectate = activity.Secrets.Spectate,
                Match = activity.Secrets.Match
            };
            if (secrets.Join != null || secrets.Spectate != null || secrets.Match != null)
                payload.Secrets = secrets;
        }

        if (activity.Buttons != null && activity.Buttons.Length > 0)
        {
            payload.Buttons = [.. activity.Buttons.Select(b => new ActivityButtonPayload
            {
                Label = b.Label,
                Url = b.Url
            })];
        }

        return payload;
    }
}

