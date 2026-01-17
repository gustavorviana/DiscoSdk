using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions
{
    internal interface IIUpdatePresenceAction : IRestAction
    {
        IUpdatePresenceAction AddActivity(Activity activity);
        IUpdatePresenceAction AddActivity(IActivity activity);
        IUpdatePresenceAction ClearActivities();
        IUpdatePresenceAction SetActivities(Activity[]? activities);
        IUpdatePresenceAction SetAfk(bool afk);
        IUpdatePresenceAction SetSince(DateTimeOffset? since);
        IUpdatePresenceAction SetStatus(OnlineStatus status);
    }
}