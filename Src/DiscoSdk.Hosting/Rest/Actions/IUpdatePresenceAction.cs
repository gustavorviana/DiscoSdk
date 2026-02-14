using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions
{
    internal interface IIUpdatePresenceAction : IRestAction
    {
        IUpdatePresenceAction AddActivity(ActivityUpdate activity);
        IUpdatePresenceAction AddActivity(IActivity activity);
        IUpdatePresenceAction ClearActivities();
        IUpdatePresenceAction SetActivities(ActivityUpdate[]? activities);
        IUpdatePresenceAction SetActivity(ActivityUpdate? activity);
        IUpdatePresenceAction SetAfk(bool afk);
        IUpdatePresenceAction SetSince(DateTimeOffset? since);
        IUpdatePresenceAction SetStatus(OnlineStatus status);
    }
}